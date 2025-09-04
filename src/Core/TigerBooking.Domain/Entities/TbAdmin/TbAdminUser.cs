using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Domain.Entities.TbAdmin;

public class TbAdminUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = string.Empty; // 관리자 권한
    public string Department { get; set; } = string.Empty; // 부서
}
