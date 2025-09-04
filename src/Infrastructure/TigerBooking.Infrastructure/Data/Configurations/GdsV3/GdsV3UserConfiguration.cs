using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.GdsV3;
using TigerBooking.Infrastructure.Data.Configurations.Common;

namespace TigerBooking.Infrastructure.Data.Configurations.GdsV3;

public class GdsV3UserConfiguration : BaseEntityConfiguration<GdsV3User>
{
    public override void Configure(EntityTypeBuilder<GdsV3User> builder)
    {
        // BaseEntity 공통 필드 매핑 먼저 적용
        base.Configure(builder);
        
        // 테이블 이름 설정
        builder.ToTable("user");
        
        // GdsV3User 엔티티의 고유한 필드들
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
            
        builder.Property(e => e.CustomerType)
            .HasColumnName("customer_type")
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);
            
        builder.Property(e => e.LastLoginAt)
            .HasColumnName("last_login_at");
            
        builder.Property(e => e.LoginAttempts)
            .HasColumnName("login_attempts")
            .HasDefaultValue(0);

        // 인덱스 설정
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.CustomerType);
        builder.HasIndex(e => e.LastLoginAt);
    }
}
