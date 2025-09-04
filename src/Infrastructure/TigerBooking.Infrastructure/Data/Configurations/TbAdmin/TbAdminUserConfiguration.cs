using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Infrastructure.Data.Configurations.Common;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

public class TbAdminUserConfiguration : BaseEntityConfiguration<TbAdminUser>
{
    public override void Configure(EntityTypeBuilder<TbAdminUser> builder)
    {
        // BaseEntity 공통 필드 매핑 먼저 적용
        base.Configure(builder);
        
        // 테이블 이름 설정 - admin_user 테이블에 매핑
        builder.ToTable("admin_user");
        
        // TbAdminUser 엔티티의 고유한 필드들
        builder.Property(e => e.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
            
        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.Department)
            .HasColumnName("department")
            .HasMaxLength(100);

        // 인덱스 설정
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Role);
    }
}
