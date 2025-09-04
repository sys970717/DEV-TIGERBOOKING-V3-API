using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Domain.Interfaces.TbAdmin;

/// <summary>
/// SocialAuth 리포지토리 인터페이스
/// SNS 인증 정보 데이터 액세스를 담당
/// </summary>
public interface ISocialAuthRepository
{
    /// <summary>
    /// Provider와 ProviderUserId로 소셜 인증 정보 조회
    /// </summary>
    Task<SocialAuth?> GetByProviderAndUserIdAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ID로 소셜 인증 정보 조회
    /// </summary>
    Task<SocialAuth?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 새 소셜 인증 정보 생성
    /// </summary>
    Task<SocialAuth> CreateAsync(SocialAuth socialAuth, CancellationToken cancellationToken = default);

    /// <summary>
    /// 소셜 인증 정보 업데이트
    /// </summary>
    Task<SocialAuth> UpdateAsync(SocialAuth socialAuth, CancellationToken cancellationToken = default);

    /// <summary>
    /// 소셜 인증 정보 소프트 삭제
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
