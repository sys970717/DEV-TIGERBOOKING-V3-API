using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Domain.Entities.GdsV3;

public class GdsV3User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string CustomerType { get; set; } = string.Empty; // 고객 타입 (개인/기업 등)
    public string PhoneNumber { get; set; } = string.Empty; // 전화번호
    public DateTime? LastLoginAt { get; set; } // 마지막 로그인 시간
    public int LoginAttempts { get; set; } = 0; // 로그인 시도 횟수
}
