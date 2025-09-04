namespace TigerBooking.Application.DTOs.GdsV3;

/// <summary>
/// GdsV3 스키마의 일반 사용자 정보를 담는 DTO입니다.
/// </summary>
public class GdsV3UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public int LoginAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// GdsV3 스키마의 일반 사용자 생성 요청 DTO입니다.
/// </summary>
public class CreateGdsV3UserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// GdsV3 스키마의 일반 사용자 업데이트 요청 DTO입니다.
/// </summary>
public class UpdateGdsV3UserDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
