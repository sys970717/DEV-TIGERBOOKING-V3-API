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
    private readonly TigerBooking.Infrastructure.Services.Redis.IRedisClient _redisClient;
    private readonly JwtSettings _jwtSettings;
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public TokenService(
        TigerBooking.Infrastructure.Services.Redis.IRedisClient redisClient,
        IOptions<JwtSettings> jwtSettings,
        IOptions<RedisSettings> redisSettings,
        ILogger<TokenService> logger)
    {
        _redisClient = redisClient;
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

    // Refresh token 생성
    var refreshToken = Guid.NewGuid().ToString();
    var refreshExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

    // Redis에 허용 리스트로 Access 토큰 등록
    await RegisterTokenInRedisAsync(jti, userId, tokenExpiry, cancellationToken);
    // Redis에 Refresh 토큰 등록(key: prefix + "refresh:" + refreshToken)
    await RegisterRefreshTokenInRedisAsync(refreshToken, userId, refreshExpiry, cancellationToken);

        _logger.LogInformation("토큰 생성 완료: UserId {UserId}, JTI {Jti}", userId, jti);

        return new TokenResponseDto
        {
            AccessToken = tokenString,
            Jti = jti,
            ExpiresIn = (int)TimeSpan.FromMinutes(_jwtSettings.AccessTokenExpirationMinutes).TotalSeconds,
            RefreshToken = refreshToken,
            RefreshExpiresIn = (int)TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays).TotalSeconds
        };
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisRefreshTokenKey(refreshToken);
            var exists = await _redisClient.StringGetAsync(redisKey);
            if (string.IsNullOrEmpty(exists))
            {
                _logger.LogWarning("Refresh token not found or expired: {RefreshToken}", refreshToken);
                return null;
            }
            var tokenInfo = !string.IsNullOrEmpty(exists) ? JsonSerializer.Deserialize<RefreshTokenInfo>(exists) : null;
            if (tokenInfo == null)
            {
                return null;
            }

            // 리플레이 탐지: 이미 revoked된 refresh token이 사용되면 사용자 전체의 refresh token을 무효화
            if (tokenInfo.Revoked)
            {
                _logger.LogWarning("Refresh token reuse detected (replay) for user {UserId}, token {RefreshToken}", tokenInfo.UserId, refreshToken);
                // 사용자 전체의 refresh 토큰 무효화
                await RevokeAllRefreshTokensForUserAsync(tokenInfo.UserId, cancellationToken);
                return null;
            }

            // 토큰 회전: 새 Access/Refresh 발급
            var newTokens = await GenerateTokenAsync(tokenInfo.UserId, tokenInfo.ChannelId, cancellationToken);

            // 기존 토큰을 revoked 상태로 표시하고 새 토큰 정보를 남김(재사용 탐지용)
            try
            {
                tokenInfo.Revoked = true;
                tokenInfo.ReplacedBy = newTokens.RefreshToken;
                tokenInfo.RevokedAt = DateTime.UtcNow;

                var ttl = tokenInfo.ExpiresAt - DateTime.UtcNow;
                if (ttl <= TimeSpan.Zero)
                {
                    // 이미 만료된 토큰이면 바로 삭제
                    await _redisClient.KeyDeleteAsync(redisKey);
                }
                else
                {
                    var json = JsonSerializer.Serialize(tokenInfo);
                    await _redisClient.StringSetAsync(redisKey, json, ttl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "기존 refresh 토큰의 revoked 표시 중 오류 발생: {RefreshToken}", refreshToken);
            }

            return newTokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token 처리 중 오류 발생");
            return null;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisRefreshTokenKey(refreshToken);
            var result = await _redisClient.KeyDeleteAsync(redisKey);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token 무효화 중 오류 발생");
            return false;
        }
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

    public Task<long?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst("sub")?.Value;

            if (long.TryParse(userIdClaim, out var userId))
            {
                return Task.FromResult<long?>(userId);
            }

            return Task.FromResult<long?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "토큰에서 사용자 ID 추출 실패");
            return Task.FromResult<long?>(null);
        }
    }

    public async Task<bool> RevokeTokenAsync(string jti, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisTokenKey(jti);
            var result = await _redisClient.KeyDeleteAsync(redisKey);
            
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
            var exists = await _redisClient.KeyExistsAsync(redisKey);
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

            await _redisClient.StringSetAsync(redisKey, tokenJson, ttl);
            _logger.LogDebug("Redis에 토큰 등록 완료: JTI {Jti}, TTL {TTL}초", jti, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis 토큰 등록 중 오류 발생: JTI {Jti}", jti);
            throw;
        }
    }

    private async Task RegisterRefreshTokenInRedisAsync(string refreshToken, long userId, DateTime expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetRedisRefreshTokenKey(refreshToken);
            var tokenInfo = new RefreshTokenInfo
            {
                UserId = userId,
                ChannelId = 1,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiry
            };

            var json = JsonSerializer.Serialize(tokenInfo);
            var ttl = expiry - DateTime.UtcNow;
            await _redisClient.StringSetAsync(redisKey, json, ttl);
            _logger.LogDebug("Redis에 Refresh 토큰 등록 완료: {Key}, TTL {TTL}초", redisKey, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis Refresh 토큰 등록 중 오류 발생");
            throw;
        }
    }

    private string GetRedisRefreshTokenKey(string refreshToken)
    {
        return $"{_redisSettings.TokenKeyPrefix}refresh:{refreshToken}";
    }

    private class RefreshTokenInfo
    {
        public long UserId { get; set; }
        public long ChannelId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        // Replay detection
        public bool Revoked { get; set; }
        public string? ReplacedBy { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

    // Revoke all refresh tokens for a user (used when a replay is detected)
    private async Task RevokeAllRefreshTokensForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Redis key pattern: prefix + "refresh:*" - scan and delete matching entries for the user
            var pattern = $"{_redisSettings.TokenKeyPrefix}refresh:*";
            var keys = await _redisClient.GetKeysAsync(pattern);
            foreach (var key in keys)
            {
                var val = await _redisClient.StringGetAsync(key);
                if (string.IsNullOrEmpty(val)) continue;
                var info = JsonSerializer.Deserialize<RefreshTokenInfo>(val);
                if (info != null && info.UserId == userId)
                {
                    await _redisClient.KeyDeleteAsync(key);
                }
            }
            _logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 전체 refresh 토큰 무효화 중 오류 발생: UserId {UserId}", userId);
        }
    }

    private string GetRedisTokenKey(string jti)
    {
        return $"{_redisSettings.TokenKeyPrefix}{jti}";
    }
}
