using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.GdsV3;
using TigerBooking.Domain.Interfaces.GdsV3;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// GdsV3 스키마의 사용자 관리를 위한 Repository 구현체입니다.
/// 일반 사용자 계정 관련 특화된 기능과 기본 CRUD 작업을 제공합니다.
/// </summary>
public class GdsV3UserRepository : BaseRepository<GdsV3User, GdsV3DbContext>, IGdsV3UserRepository
{
    public GdsV3UserRepository(GdsV3DbContext context) : base(context)
    {
    }

    public async Task<GdsV3User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Username == username, cancellationToken);
    }

    public async Task<GdsV3User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .AnyAsync(e => e.Username == username, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .AnyAsync(e => e.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<GdsV3User>> GetByCustomerTypeAsync(string customerType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.CustomerType == customerType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GdsV3User>> GetRecentlyLoggedInUsersAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.LastLoginAt >= since)
            .OrderByDescending(e => e.LastLoginAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GdsV3User>> GetUsersWithMultipleLoginAttemptsAsync(int minAttempts, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.LoginAttempts >= minAttempts)
            .OrderByDescending(e => e.LoginAttempts)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateLastLoginAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LoginAttempts = 0; // 성공적인 로그인 시 시도 횟수 초기화
        }
    }

    public async Task IncrementLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LoginAttempts++;
        }
    }

    public async Task ResetLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LoginAttempts = 0;
        }
    }
}
