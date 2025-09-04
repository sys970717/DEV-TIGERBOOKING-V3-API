using TigerBooking.Domain.Entities.GdsV3;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Domain.Interfaces.GdsV3;

/// <summary>
/// GdsV3 스키마의 사용자 관리를 위한 Repository 인터페이스입니다.
/// 일반 사용자 계정 관련 특화된 기능을 제공합니다.
/// </summary>
public interface IGdsV3UserRepository : IRepository<GdsV3User>
{
    Task<GdsV3User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<GdsV3User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3User>> GetByCustomerTypeAsync(string customerType, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3User>> GetRecentlyLoggedInUsersAsync(DateTime since, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3User>> GetUsersWithMultipleLoginAttemptsAsync(int minAttempts, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(long userId, CancellationToken cancellationToken = default);
    Task IncrementLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default);
    Task ResetLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default);
}
