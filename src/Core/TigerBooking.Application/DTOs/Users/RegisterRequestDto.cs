namespace TigerBooking.Application.DTOs.Users;

/// <summary>
/// 회원가입 요청 DTO
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// 이메일 주소
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 비밀번호
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 성 (여권식)
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// 이름 (여권식)
    /// </summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>
    /// 성별 (선택)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 닉네임 (선택)
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 전화번호 국가코드 (선택)
    /// </summary>
    public string? PhoneCountryCode { get; set; }

    /// <summary>
    /// 전화번호 (선택)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 국적 코드 (선택)
    /// </summary>
    public string? NationalityCode { get; set; }
}
