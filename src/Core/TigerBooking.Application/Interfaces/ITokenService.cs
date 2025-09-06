namespace TigerBooking.Application.Interfaces;

/// <summary>
/// JWT 토큰 관리를 위한 서비스 인터페이스입니다.
/// Redis를 통해 토큰 상태를 관리하고, 자동로그인 기능 확장을 고려한 설계입니다.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 사용자 ID와 채널 ID로 JWT 토큰을 생성합니다.
    /// </summary>
    Task<TokenResponseDto> GenerateTokenAsync(long userId, long channelId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 토큰의 유효성을 검증합니다.
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 토큰에서 사용자 ID를 추출합니다.
    /// </summary>
    Task<long?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 토큰을 무효화합니다 (로그아웃).
    /// </summary>
    Task<bool> RevokeTokenAsync(string jti, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Redis에서 토큰 상태를 확인합니다.
    /// </summary>
    Task<bool> IsTokenValidInRedisAsync(string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token으로 새로운 Access/Refresh 토큰을 발급합니다 (토큰 회전).
    /// </summary>
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token을 무효화합니다.
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// 토큰 응답 DTO
/// </summary>
public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string Jti { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public int RefreshExpiresIn { get; set; }
}
