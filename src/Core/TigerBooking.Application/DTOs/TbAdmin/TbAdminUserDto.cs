namespace TigerBooking.Application.DTOs.TbAdmin;

/// <summary>
/// TbAdmin 스키마의 관리자 사용자 정보를 담는 DTO입니다.
/// </summary>
public class TbAdminUserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// TbAdmin 스키마의 관리자 사용자 생성 요청 DTO입니다.
/// </summary>
public class CreateTbAdminUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// TbAdmin 스키마의 관리자 사용자 업데이트 요청 DTO입니다.
/// </summary>
public class UpdateTbAdminUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
