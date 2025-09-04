using System.ComponentModel.DataAnnotations.Schema;
using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Domain.Entities.TbAdmin;

/// <summary>
/// 최종 사용자(B2C 고객) 엔티티
/// 이메일/이름/성별/닉네임/전화/국적은 암호화하여 저장
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// 채널 ID (현재는 1 고정, 향후 환경설정으로 변경 예정)
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// 이메일 주소 (암호화 저장)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 비밀번호 해시 (LOCAL 계정만, bcrypt/Argon2id 사용)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// SNS 인증 연결 인덱스 (논리적 FK, 물리적 FK 없음)
    /// </summary>
    public long? SocialAuthIdx { get; set; }

    /// <summary>
    /// 성 (여권식, 암호화 저장)
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// 이름 (여권식, 암호화 저장)
    /// </summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>
    /// 성별 (암호화 저장)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 닉네임 (암호화 저장)
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 전화번호 국가코드 (암호화 저장)
    /// </summary>
    public string? PhoneCountryCode { get; set; }

    /// <summary>
    /// 전화번호 (암호화 저장)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 국적 코드 (암호화 저장)
    /// </summary>
    public string? NationalityCode { get; set; }

    /// <summary>
    /// 포인트 잔액
    /// </summary>
    public decimal Point { get; set; } = 0;

    /// <summary>
    /// 활성 여부
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 이메일 인증 완료 시각
    /// </summary>
    public DateTime? EmailVerifiedTz { get; set; }

    /// <summary>
    /// 마지막 로그인 시각
    /// </summary>
    public DateTime? LastLoginTz { get; set; }

    /// <summary>
    /// 로그인 실패 횟수
    /// </summary>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>
    /// 계정 잠금 해제 시각
    /// </summary>
    public DateTime? LockedUntilTz { get; set; }

    // Navigation properties (논리적 관계, 물리적 FK 없음)
    public Channel? Channel { get; set; }
    public SocialAuth? SocialAuth { get; set; }
}
