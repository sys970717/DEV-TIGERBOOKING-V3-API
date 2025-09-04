using TigerBooking.Application.DTOs.GdsV3;

namespace TigerBooking.Application.Interfaces;

/// <summary>
/// GdsV3 스키마의 일반 사용자 관리를 위한 서비스 인터페이스입니다.
/// 일반 사용자 계정 관련 비즈니스 로직을 제공합니다.
/// </summary>
public interface IGdsV3UserService
{
    Task<GdsV3UserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<GdsV3UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<GdsV3UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3UserDto>> GetByCustomerTypeAsync(string customerType, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3UserDto>> GetRecentlyLoggedInUsersAsync(DateTime since, CancellationToken cancellationToken = default);
    Task<IEnumerable<GdsV3UserDto>> GetUsersWithMultipleLoginAttemptsAsync(int minAttempts, CancellationToken cancellationToken = default);
    Task<GdsV3UserDto> CreateAsync(CreateGdsV3UserDto createDto, CancellationToken cancellationToken = default);
    Task<GdsV3UserDto> UpdateAsync(long id, UpdateGdsV3UserDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken = default);
    Task HandleLoginSuccessAsync(long userId, CancellationToken cancellationToken = default);
    Task HandleLoginFailureAsync(long userId, CancellationToken cancellationToken = default);
    Task ResetLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default);
}
