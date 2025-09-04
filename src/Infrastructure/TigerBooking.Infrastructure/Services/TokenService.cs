using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TigerBooking.Application.Configuration;
using TigerBooking.Application.Interfaces;

namespace TigerBooking.Infrastructure.Services;

/// <summary>
/// JWT + Redis 기반 토큰 관리 서비스입니다.
/// Access Token을 관리하며, 추후 Refresh Token 확장이 가능한 구조입니다.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly JwtSettings _jwtSettings;
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public TokenService(
        IConnectionMultiplexer redis,
        IOptions<JwtSettings> jwtSettings,
        IOptions<RedisSettings> redisSettings,
        ILogger<TokenService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _jwtSettings = jwtSettings.Value;
        _redisSettings = redisSettings.Value;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<TokenResponseDto> GenerateTokenAsync(long userId, long channelId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("토큰 생성 시작: UserId {UserId}, ChannelId {ChannelId}", userId, channelId);

        var tokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        var jti = Guid.NewGuid().ToString(); // JWT ID for token tracking

        var claims = new[]
        {
            new Claim("sub", userId.ToString()), // Subject (사용자 ID)
            new Claim("ch", channelId.ToString()), // Channel ID
            new Claim("jti", jti), // JWT ID
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("exp", ((DateTimeOffset)tokenExpiry).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: tokenExpiry,
            signingCredentials: credentials
        );

        var tokenString = _tokenHandler.WriteToken(token);

        // Redis에 허용 리스트로 토큰 등록
        await RegisterTokenInRedisAsync(jti, userId, tokenExpiry, cancellationToken);

        _logger.LogInformation("토큰 생성 완료: UserId {UserId}, JTI {Jti}", userId, jti);

        return new TokenResponseDto
        {
            AccessToken = tokenString,
            Jti = jti,
            ExpiresIn = (int)TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpirationMinutes).TotalSeconds
        };
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            var jti = principal.FindFirst("jti")?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                return false;
            }

            // Redis에서 토큰 상태 확인
            return await IsTokenValidInRedisAsync(jti, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "토큰 검증 실패: {Token}", token[..Math.Min(token.Length, 20)]);
            return false;
        }
    }

    public async Task<long?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst("sub")?.Value;

            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "토큰에서 사용자 ID 추출 실패");
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string jti, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisTokenKey(jti);
            var result = await _database.KeyDeleteAsync(redisKey);
            
            if (result)
            {
                _logger.LogInformation("토큰 무효화 성공: JTI {Jti}", jti);
            }
            else
            {
                _logger.LogWarning("토큰 무효화 실패 - 토큰이 존재하지 않음: JTI {Jti}", jti);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "토큰 무효화 중 오류 발생: JTI {Jti}", jti);
            return false;
        }
    }

    public async Task<bool> IsTokenValidInRedisAsync(string jti, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisTokenKey(jti);
            var exists = await _database.KeyExistsAsync(redisKey);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis 토큰 상태 확인 중 오류 발생: JTI {Jti}", jti);
            return false;
        }
    }

    // Private helper methods
    private async Task RegisterTokenInRedisAsync(string jti, long userId, DateTime expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisTokenKey(jti);
            var tokenInfo = new
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiry
            };

            var tokenJson = JsonSerializer.Serialize(tokenInfo);
            var ttl = expiry - DateTime.UtcNow;

            await _database.StringSetAsync(redisKey, tokenJson, ttl);
            _logger.LogDebug("Redis에 토큰 등록 완료: JTI {Jti}, TTL {TTL}초", jti, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis 토큰 등록 중 오류 발생: JTI {Jti}", jti);
            throw;
        }
    }

    private string GetRedisTokenKey(string jti)
    {
        return $"{_redisSettings.TokenKeyPrefix}{jti}";
    }
}
