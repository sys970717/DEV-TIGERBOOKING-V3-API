using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// TbAdmin 스키마의 사용자 관리를 위한 Repository 구현체입니다.
/// 관리자 계정 관련 특화된 기능과 기본 CRUD 작업을 제공합니다.
/// </summary>
public class TbAdminUserRepository : BaseRepository<TbAdminUser, TbAdminDbContext>, ITbAdminUserRepository
{
    public TbAdminUserRepository(TbAdminDbContext context) : base(context)
    {
    }

    public async Task<TbAdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Username == username, cancellationToken);
    }

    public async Task<TbAdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<TbAdminUser>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TbAdminUser>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted && e.Department == department)
            .ToListAsync(cancellationToken);
    }
}
