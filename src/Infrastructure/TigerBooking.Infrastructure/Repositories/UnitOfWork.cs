using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TigerBooking.Infrastructure.Data;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// 제네릭 UnitOfWork 패턴 구현체입니다.
/// 여러 Repository 간의 트랜잭션을 관리하고, 데이터 일관성을 보장합니다.
/// 각 스키마별 DbContext에서 사용할 수 있도록 제네릭으로 구현되었습니다.
/// </summary>
/// <typeparam name="TContext">BaseDbContext를 상속받는 DbContext 타입</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork where TContext : BaseDbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
