using TigerBooking.Application.DTOs.TbAdmin;

namespace TigerBooking.Application.Interfaces;

/// <summary>
/// 채널 서비스 인터페이스
/// </summary>
public interface IChannelService
{
    /// <summary>
    /// 채널 목록 조회
    /// </summary>
    Task<GetChannelsResponseDto> GetChannelsAsync(GetChannelsRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 상세 조회
    /// </summary>
    Task<ChannelDto?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 생성
    /// </summary>
    Task<ChannelDto> CreateChannelAsync(CreateChannelRequestDto request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 수정
    /// </summary>
    Task<ChannelDto> UpdateChannelAsync(long id, UpdateChannelRequestDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 삭제 (소프트 삭제)
    /// </summary>
    Task DeleteChannelAsync(long id, string deletedBy, CancellationToken cancellationToken = default);
}
