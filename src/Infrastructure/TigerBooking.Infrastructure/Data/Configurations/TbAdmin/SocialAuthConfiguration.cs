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

        // 최소 컬럼 매핑(snake_case)
        builder.Property(e => e.Provider).HasColumnName("provider");
        builder.Property(e => e.ProviderUserId).HasColumnName("provider_user_id");
        builder.Property(e => e.ProviderEmail).HasColumnName("provider_email");
        builder.Property(e => e.ProviderToken).HasColumnName("provider_token");

        // BaseEntity 공통 컬럼 명시 매핑
        builder.Property(e => e.CreatedTz).HasColumnName("created_tz");
        builder.Property(e => e.UpdatedTz).HasColumnName("updated_tz");
        builder.Property(e => e.CreatedBy).HasColumnName("created_by");
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedTz).HasColumnName("deleted_tz");
        builder.Property(e => e.DeletedBy).HasColumnName("deleted_by");

        // 유일성 제약조건: (provider, provider_user_id)는 소프트삭제 제외 전역 유일
        builder.HasIndex(e => new { e.Provider, e.ProviderUserId })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("uk_social_auth_provider_user");

    // Navigation을 명시적으로 무시하여 EF Core가 User 쪽에 shadow FK(SocialAuthId)를 생성하지 않도록 함
    // 애플리케이션 레벨에서 SocialAuthIdx를 통해 논리적 관계를 관리함(물리적 FK 미생성)
    builder.Ignore(e => e.Users);
    }
}
