using TigerBooking.Domain.Entities.TbAdmin;

namespace TigerBooking.Domain.Interfaces.TbAdmin;

/// <summary>
/// User 리포지토리 인터페이스
/// B2C 고객 사용자 데이터 액세스를 담당
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 이메일과 채널 ID로 사용자 조회 (소프트삭제 제외)
    /// </summary>
    Task<User?> GetByEmailAndChannelAsync(string email, long channelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 이메일과 채널 ID로 활성 채널의 사용자 조회 (채널도 활성 상태여야 함)
    /// </summary>
    Task<User?> GetByEmailAndActiveChannelAsync(string email, long channelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 ID로 조회
    /// </summary>
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 ID로 활성 채널의 사용자 조회 (채널도 활성 상태여야 함)
    /// </summary>
    Task<User?> GetByIdWithActiveChannelAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 새 사용자 생성
    /// </summary>
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 정보 업데이트
    /// </summary>
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 소프트 삭제
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 마지막 로그인 시간 업데이트
    /// </summary>
    Task UpdateLastLoginAsync(long id, DateTime loginTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그인 실패 횟수 증가
    /// </summary>
    Task IncrementFailedLoginCountAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그인 실패 횟수 초기화
    /// </summary>
    Task ResetFailedLoginCountAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 계정 잠금 설정
    /// </summary>
    Task LockAccountAsync(long id, DateTime lockUntil, CancellationToken cancellationToken = default);
}
