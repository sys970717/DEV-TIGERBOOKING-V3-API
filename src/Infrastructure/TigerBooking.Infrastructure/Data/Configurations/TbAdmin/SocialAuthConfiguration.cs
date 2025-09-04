using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

/// <summary>
/// SocialAuth 엔티티에 대한 EF Core 구성
/// SNS 인증 정보 저장을 위한 테이블 설정
/// </summary>
public class SocialAuthConfiguration : IEntityTypeConfiguration<SocialAuth>
{
    public void Configure(EntityTypeBuilder<SocialAuth> builder)
    {
        builder.ToTable("social_auth", "tb_admin"); // 스키마 명시적 지정

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 소셜 공급자
        builder.Property(e => e.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        // 공급자 내 사용자 ID
        builder.Property(e => e.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(200)
            .IsRequired();

        // 공급자 이메일 (암호화 저장 가능)
        builder.Property(e => e.ProviderEmail)
            .HasColumnName("provider_email")
            .HasMaxLength(500);

        // 공급자 토큰 (민감 정보, 암호화 권장)
        builder.Property(e => e.ProviderToken)
            .HasColumnName("provider_token")
            .HasMaxLength(2000);

        // BaseEntity 필드들
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        // 유일성 제약조건: (provider, provider_user_id)는 소프트삭제 제외 전역 유일
        builder.HasIndex(e => new { e.Provider, e.ProviderUserId })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("uk_social_auth_provider_user");
    }
}
