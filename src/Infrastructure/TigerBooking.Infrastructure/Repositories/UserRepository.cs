using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// User 리포지토리 구현체
/// TbAdmin 스키마의 user 테이블에 대한 데이터 액세스 제공
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly TbAdminDbContext _context;

    public UserRepository(TbAdminDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAndChannelAsync(string email, long channelId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Email == email && u.ChannelId == channelId && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAndActiveChannelAsync(string email, long channelId, CancellationToken cancellationToken = default)
    {
        // Channel의 활성 상태를 별도로 확인
        var channelExists = await _context.Channels
            .AnyAsync(c => c.Id == channelId && !c.IsDeleted, cancellationToken);
            
        if (!channelExists)
        {
            return null; // 채널이 존재하지 않거나 비활성화됨
        }
        
        return await _context.Users
            .Where(u => u.Email == email && 
                       u.ChannelId == channelId && 
                       !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByIdWithActiveChannelAsync(long id, CancellationToken cancellationToken = default)
    {
        // 먼저 사용자를 조회
        var user = await _context.Users
            .Where(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (user == null)
        {
            return null;
        }
        
        // 해당 사용자의 채널이 활성 상태인지 확인
        var channelExists = await _context.Channels
            .AnyAsync(c => c.Id == user.ChannelId && !c.IsDeleted, cancellationToken);
            
        return channelExists ? user : null;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateLastLoginAsync(long id, DateTime loginTime, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.LastLoginTz = loginTime;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementFailedLoginCountAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.FailedLoginCount++;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResetFailedLoginCountAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.FailedLoginCount = 0;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LockAccountAsync(long id, DateTime lockUntil, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            user.LockedUntilTz = lockUntil;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
