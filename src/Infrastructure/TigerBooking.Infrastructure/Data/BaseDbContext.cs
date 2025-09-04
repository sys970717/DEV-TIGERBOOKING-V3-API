using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Infrastructure.Data;

public abstract class BaseDbContext : DbContext
{
    protected readonly string SchemaName;

    protected BaseDbContext(DbContextOptions options, string schemaName) : base(options)
    {
        SchemaName = schemaName;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: CreatedBy는 현재 사용자 정보로 설정
                entity.CreatedBy = "system"; // 임시값
                entity.UpdatedBy = "system"; // 임시값
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: UpdatedBy는 현재 사용자 정보로 설정
                entity.UpdatedBy = "system"; // 임시값
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PostgreSQL 스키마 설정 - 각 엔티티에 개별적으로 적용
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            entityType.SetSchema(SchemaName);
        }
        
        base.OnModelCreating(modelBuilder);
    }
}
