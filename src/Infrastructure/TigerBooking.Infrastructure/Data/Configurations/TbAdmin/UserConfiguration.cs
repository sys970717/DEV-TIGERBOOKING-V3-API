using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Infrastructure.Data.Configurations.TbAdmin;

/// <summary>
/// User 엔티티에 대한 EF Core 구성
/// 암호화된 필드들은 VARCHAR(500)으로 설정
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user", "tb_admin"); // 스키마 명시적 지정

        // Primary Key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // 채널 ID
        builder.Property(e => e.ChannelId)
            .HasColumnName("channel_id")
            .IsRequired();

        // 이메일 (암호화 저장)
        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(500)
            .IsRequired();

        // 비밀번호 해시
        builder.Property(e => e.Password)
            .HasColumnName("password")
            .HasMaxLength(100);

        // SNS 인증 연결 인덱스
        builder.Property(e => e.SocialAuthIdx)
            .HasColumnName("social_auth_idx");

        // 성/이름 (암호화 저장)
        builder.Property(e => e.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.GivenName)
            .HasColumnName("given_name")
            .HasMaxLength(500)
            .IsRequired();

        // 성별 (암호화 저장)
        builder.Property(e => e.Gender)
            .HasColumnName("gender")
            .HasMaxLength(500);

        // 닉네임 (암호화 저장)
        builder.Property(e => e.Nickname)
            .HasColumnName("nickname")
            .HasMaxLength(500);

        // 전화번호 관련 (암호화 저장)
        builder.Property(e => e.PhoneCountryCode)
            .HasColumnName("phone_country_code")
            .HasMaxLength(500);

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(500);

        // 국적 (암호화 저장)
        builder.Property(e => e.NationalityCode)
            .HasColumnName("nationality_code")
            .HasMaxLength(500);

        // 포인트
        builder.Property(e => e.Point)
            .HasColumnName("point")
            .HasColumnType("decimal(15,6)")
            .HasDefaultValue(0)
            .IsRequired();

        // 활성 여부
        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // 보안/상태 관련 필드들
        builder.Property(e => e.EmailVerifiedTz)
            .HasColumnName("email_verified_tz");

        builder.Property(e => e.LastLoginTz)
            .HasColumnName("last_login_tz");

        builder.Property(e => e.FailedLoginCount)
            .HasColumnName("failed_login_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.LockedUntilTz)
            .HasColumnName("locked_until_tz");

        // BaseEntity 필드들은 BaseDbContext에서 공통으로 처리하므로 여기서 제거

        // 유일성 제약조건: 같은 채널에서 이메일 중복 금지 (소프트삭제 제외)
        builder.HasIndex(e => new { e.ChannelId, e.Email })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("uk_user_channel_email");

        // 논리적 관계 설정 (물리적 FK 없음) - Channel과의 관계를 선택적으로 설정
        builder.HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false); // 선택적 관계로 변경

        builder.HasOne(e => e.SocialAuth)
            .WithMany(s => s.Users)
            .HasForeignKey(e => e.SocialAuthIdx)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false) // 선택적 관계로 변경
            .HasConstraintName(null); // 물리적 FK 생성하지 않음
    }
}
