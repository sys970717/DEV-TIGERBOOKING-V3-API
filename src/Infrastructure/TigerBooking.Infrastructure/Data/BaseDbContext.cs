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
                entity.CreatedTz = DateTime.UtcNow;
                entity.UpdatedTz = DateTime.UtcNow;
                // TODO: CreatedBy는 현재 사용자 정보로 설정
                entity.CreatedBy = "system"; // 임시값
                entity.UpdatedBy = "system"; // 임시값
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedTz = DateTime.UtcNow;
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
        
        // BaseEntity를 상속하는 모든 엔티티에 대한 공통 컬럼 매핑 설정 (먼저 적용)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);
                
                // 강제로 모든 BaseEntity 속성들을 snake_case로 매핑
                entity.Property("Id").HasColumnName("id");
                entity.Property("CreatedTz").HasColumnName("created_tz");
                entity.Property("UpdatedTz").HasColumnName("updated_tz");
                entity.Property("CreatedBy").HasColumnName("created_by").HasMaxLength(100);
                entity.Property("UpdatedBy").HasColumnName("updated_by").HasMaxLength(100);
                entity.Property("IsDeleted").HasColumnName("is_deleted");
                entity.Property("DeletedTz").HasColumnName("deleted_tz");
                entity.Property("DeletedBy").HasColumnName("deleted_by").HasMaxLength(100);
            }
        }
        
        // 개별 Configuration들은 나중에 적용되어 BaseEntity 외의 속성들을 설정
        base.OnModelCreating(modelBuilder);
    }
}
