namespace TigerBooking.Application.DTOs.Users;

/// <summary>
/// 사용자 정보 응답 DTO
/// 내 정보 조회 시 사용
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// 사용자 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 채널 ID
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// 이메일 주소 (복호화된 값)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 성 (복호화된 값)
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// 이름 (복호화된 값)
    /// </summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>
    /// 성별 (복호화된 값)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 닉네임 (복호화된 값)
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 전화번호 국가코드 (복호화된 값)
    /// </summary>
    public string? PhoneCountryCode { get; set; }

    /// <summary>
    /// 전화번호 (복호화된 값)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 국적 코드 (복호화된 값)
    /// </summary>
    public string? NationalityCode { get; set; }

    /// <summary>
    /// 포인트 잔액
    /// </summary>
    public decimal Point { get; set; }

    /// <summary>
    /// 활성 여부
    /// </summary>
    public bool IsActive { get; set; }
}
