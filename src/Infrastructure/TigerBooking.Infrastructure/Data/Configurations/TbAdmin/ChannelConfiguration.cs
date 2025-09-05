using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("channel", "tb_admin");

        // PK
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 최소 컬럼 매핑(snake_case)
        builder.Property(x => x.ParentChannelId).HasColumnName("parent_channel_id");
        builder.Property(x => x.Code).HasColumnName("code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.ContractDate).HasColumnName("contract_date");
        builder.Property(x => x.Ratio).HasColumnName("ratio");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");

        // BaseEntity(삭제 관련) 명시 매핑 유지
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.DeletedTz).HasColumnName("deleted_tz");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by");

        // 인덱스(최소)
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("parent_channel_id IS NULL")
            .HasDatabaseName("IX_channel_code_root_unique");

        // 서브 채널 코드 유니크 (동일 부모 내)
        builder.HasIndex(x => new { x.ParentChannelId, x.Code })
            .IsUnique()
            .HasFilter("parent_channel_id IS NOT NULL")
            .HasDatabaseName("IX_channel_parent_code_unique");

        // 조회용 인덱스
        builder.HasIndex(x => new { x.ParentChannelId, x.SortOrder })
            .HasDatabaseName("IX_channel_parent_sort");

        // 관계/쿼리필터/디폴트/길이 등 상세 설정은 생략(논리적 관계만 유지)
    }
}
