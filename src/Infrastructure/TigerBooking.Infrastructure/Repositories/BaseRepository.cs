using Microsoft.EntityFrameworkCore;
using TigerBooking.Infrastructure.Data;
using TigerBooking.Domain.Common.Entities;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// 모든 Repository의 기본 구현체입니다.
/// BaseEntity를 상속받는 엔티티에 대한 기본 CRUD 작업을 제공합니다.
/// 각 스키마별 DbContext에서 사용할 수 있도록 제네릭으로 구현되었습니다.
/// </summary>
/// <typeparam name="T">BaseEntity를 상속받는 엔티티 타입</typeparam>
/// <typeparam name="TContext">BaseDbContext를 상속받는 DbContext 타입</typeparam>
public class BaseRepository<T, TContext> : IRepository<T> 
    where T : BaseEntity 
    where TContext : BaseDbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;

    public BaseRepository(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            // TODO: DeletedBy는 현재 사용자 정보로 설정
            entity.DeletedBy = "system"; // 임시값
        }
    }

    public virtual async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .AnyAsync(e => e.Id == id, cancellationToken);
    }
}
