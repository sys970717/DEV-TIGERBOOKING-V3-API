namespace TigerBooking.Application.DTOs.Users;

/// <summary>
/// 로그인 응답 DTO
/// JWT 토큰과 관련 정보를 포함
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// JWT Access Token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 토큰 타입 (Bearer)
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// 토큰 만료 시간 (초)
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// JWT ID (로그아웃/블랙리스트 관리용)
    /// </summary>
    public string Jti { get; set; } = string.Empty;

    /// <summary>
    /// Refresh 토큰 (토큰 재발급용)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Refresh 토큰 만료 시간 (초)
    /// </summary>
    public int RefreshExpiresIn { get; set; }
}
