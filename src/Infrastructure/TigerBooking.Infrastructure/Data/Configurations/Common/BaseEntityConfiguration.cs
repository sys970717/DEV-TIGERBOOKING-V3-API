using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Common.Entities;

namespace TigerBooking.Infrastructure.Data.Configurations.Common;

/// <summary>
/// BaseEntity를 상속받는 모든 엔티티의 공통 필드 매핑을 처리하는 기본 Configuration
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> 
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // 마이그레이션에서 제외 - DB 스키마 변경 방지
        builder.HasAnnotation("Relational:IsTableExcludedFromMigrations", true);
        
        // BaseEntity 공통 필드 매핑
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(e => e.CreatedTz)
            .HasColumnName("created_tz")
            .HasDefaultValueSql("TIMEZONE('UTC', CURRENT_TIMESTAMP)");
            
        builder.Property(e => e.UpdatedTz)
            .HasColumnName("updated_tz")
            .HasDefaultValueSql("TIMEZONE('UTC', CURRENT_TIMESTAMP)");
            
        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
            
        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(50);
            
        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);
            
        builder.Property(e => e.DeletedTz)
            .HasColumnName("deleted_tz");
            
        builder.Property(e => e.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(100);
    }
}
