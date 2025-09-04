using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Domain.Entities.TbAdmin;

/// <summary>
/// SNS 인증 정보 엔티티 (Google, Kakao, Naver, Apple 등)
/// </summary>
public class SocialAuth : BaseEntity
{
    /// <summary>
    /// 소셜 공급자 (GOOGLE, KAKAO, NAVER, APPLE 등)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 공급자 내 사용자 고유 ID
    /// </summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// 공급자에서 제공하는 이메일 (필요 시 암호화 저장)
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// 공급자 토큰/자격 정보 (민감 정보, 암호화 권장)
    /// </summary>
    public string? ProviderToken { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
