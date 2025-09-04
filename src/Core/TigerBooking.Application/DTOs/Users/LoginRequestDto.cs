namespace TigerBooking.Application.DTOs.Users;

/// <summary>
/// 로그인 요청 DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// 이메일 주소
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 비밀번호
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
