using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

/// <summary>
/// 채널 엔티티 구성
/// </summary>
public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("channel", "tb_admin");

        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(x => x.ParentChannelId)
            .HasColumnName("parent_channel_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.ContractDate)
            .HasColumnName("contract_date");

        builder.Property(x => x.Ratio)
            .HasColumnName("ratio")
            .HasPrecision(15, 6)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();
        

        // Indexes and Constraints
        // 루트 채널 코드 유니크 (parent_channel_id IS NULL인 경우)
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

        // 소프트 삭제 필터
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Self-referencing relationship
        builder.HasOne(x => x.ParentChannel)
            .WithMany(x => x.SubChannels)
            .HasForeignKey(x => x.ParentChannelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
