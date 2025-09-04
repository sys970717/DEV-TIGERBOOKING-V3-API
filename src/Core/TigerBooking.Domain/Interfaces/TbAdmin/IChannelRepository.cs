using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Domain.Interfaces.TbAdmin;

/// <summary>
/// 채널 리포지토리 인터페이스
/// </summary>
public interface IChannelRepository
{
    /// <summary>
    /// 채널 목록 조회 (페이징, 필터링, 정렬 지원)
    /// </summary>
    Task<(IEnumerable<Channel> Items, int Total)> GetChannelsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isActive = null,
        bool parentOnly = false,
        long? parentId = null,
        string? code = null,
        string? name = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ID로 채널 조회
    /// </summary>
    Task<Channel?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 코드로 채널 조회 (유니크 체크용)
    /// </summary>
    Task<Channel?> GetChannelByCodeAsync(string code, long? parentChannelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 생성
    /// </summary>
    Task<Channel> CreateChannelAsync(Channel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 수정
    /// </summary>
    Task<Channel> UpdateChannelAsync(Channel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// 채널 소프트 삭제
    /// </summary>
    Task DeleteChannelAsync(long id, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// 자식 채널 개수 조회 (삭제 전 체크용)
    /// </summary>
    Task<int> GetSubChannelCountAsync(long parentChannelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 부모 채널이 루트 채널인지 확인
    /// </summary>
    Task<bool> IsRootChannelAsync(long channelId, CancellationToken cancellationToken = default);
}
