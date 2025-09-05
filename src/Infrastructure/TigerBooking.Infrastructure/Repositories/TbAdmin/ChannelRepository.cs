using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories.TbAdmin;

/// <summary>
/// 채널 리포지토리 구현체
/// </summary>
public class ChannelRepository : IChannelRepository
{
    private readonly TbAdminDbContext _context;

    public ChannelRepository(TbAdminDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Channel> Items, int Total)> GetChannelsAsync(
        int page = 1,
        int pageSize = 20,
        bool? isActive = null,
        bool parentOnly = false,
        long? parentId = null,
        string? code = null,
        string? name = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Channels.AsQueryable();

        // 필터 적용
        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (parentOnly)
        {
            query = query.Where(c => c.ParentChannelId == null);
        }

        if (parentId.HasValue)
        {
            query = query.Where(c => c.ParentChannelId == parentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(c => c.Code.Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(c => c.ContractDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(c => c.ContractDate <= dateTo.Value);
        }

        // 정렬
        query = query.OrderBy(c => c.ParentChannelId)
                    .ThenBy(c => c.SortOrder)
                    .ThenBy(c => c.Name);

        // 총 개수 조회
        var total = await query.CountAsync(cancellationToken);

        // 페이징 적용
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Channel?> GetChannelByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Channel?> GetChannelByCodeAsync(string code, long? parentChannelId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Code == code && c.ParentChannelId == parentChannelId, cancellationToken);
    }

    public async Task<Channel> CreateChannelAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _context.Channels.Add(channel);
        await _context.SaveChangesAsync(cancellationToken);
        return channel;
    }

    public async Task<Channel> UpdateChannelAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _context.Channels.Update(channel);
        await _context.SaveChangesAsync(cancellationToken);
        return channel;
    }

    public async Task DeleteChannelAsync(long id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (channel != null)
        {
            channel.IsDeleted = true;
            channel.DeletedTz = DateTime.UtcNow;
            channel.DeletedBy = deletedBy;
            channel.UpdatedTz = DateTime.UtcNow;
            channel.UpdatedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetSubChannelCountAsync(long parentChannelId, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .CountAsync(c => c.ParentChannelId == parentChannelId, cancellationToken);
    }

    public async Task<bool> IsRootChannelAsync(long channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId, cancellationToken);

        return channel?.ParentChannelId == null;
    }
}
