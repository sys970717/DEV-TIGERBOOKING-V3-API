namespace TigerBooking.Application.DTOs.Users;

/// <summary>
/// 회원가입 응답 DTO
/// </summary>
public class RegisterResponseDto
{
    /// <summary>
    /// 생성된 사용자 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 채널 ID
    /// </summary>
    public long ChannelId { get; set; }
}
