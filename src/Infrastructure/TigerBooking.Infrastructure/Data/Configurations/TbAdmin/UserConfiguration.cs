using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

/// <summary>
/// User 엔티티 매핑 - 최소한의 컬럼명 매핑과 인덱스만 유지(논리적 관계만 사용)
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user", "tb_admin");

        // PK
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 필수 컬럼명 매핑만 적용 (snake_case)
        builder.Property(e => e.ChannelId).HasColumnName("channel_id");
        builder.Property(e => e.Email).HasColumnName("email");
        builder.Property(e => e.Password).HasColumnName("password");
        builder.Property(e => e.SocialAuthIdx).HasColumnName("social_auth_idx");
        builder.Property(e => e.FamilyName).HasColumnName("family_name");
        builder.Property(e => e.GivenName).HasColumnName("given_name");
        builder.Property(e => e.Gender).HasColumnName("gender");
        builder.Property(e => e.Nickname).HasColumnName("nickname");
        builder.Property(e => e.PhoneCountryCode).HasColumnName("phone_country_code");
        builder.Property(e => e.PhoneNumber).HasColumnName("phone_number");
        builder.Property(e => e.NationalityCode).HasColumnName("nationality_code");
        builder.Property(e => e.Point).HasColumnName("point");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.EmailVerifiedTz).HasColumnName("email_verified_tz");
        builder.Property(e => e.LastLoginTz).HasColumnName("last_login_tz");
        builder.Property(e => e.FailedLoginCount).HasColumnName("failed_login_count");
        builder.Property(e => e.LockedUntilTz).HasColumnName("locked_until_tz");

        // BaseEntity 공통(삭제 관련) 컬럼은 명시 매핑 유지 (오류 회귀 방지)
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        builder.Property(e => e.DeletedTz).HasColumnName("deleted_tz");
        builder.Property(e => e.DeletedBy).HasColumnName("deleted_by");

        // 유니크 인덱스 (소프트삭제 제외)
        builder.HasIndex(e => new { e.ChannelId, e.Email })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("uk_user_channel_email");
    // Navigation을 명시적으로 무시하여 EF가 shadow FK(SocialAuthId)를 생성하지 않도록 함
    // 논리적 관계는 도메인 레벨에서 관리하고, DB 물리적 FK는 생성하지 않음
    builder.Ignore(e => e.SocialAuth);

    // 관계 설정은 생략(논리적 관계만 사용, 물리 FK 미생성)
    }
}
