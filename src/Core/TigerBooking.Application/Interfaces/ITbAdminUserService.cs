using TigerBooking.Application.DTOs.TbAdmin;

namespace TigerBooking.Application.Interfaces;

/// <summary>
/// TbAdmin 스키마의 관리자 사용자 관리를 위한 서비스 인터페이스입니다.
/// 관리자 계정 관련 비즈니스 로직을 제공합니다.
/// </summary>
public interface ITbAdminUserService
{
    Task<TbAdminUserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<TbAdminUserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<TbAdminUserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<TbAdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TbAdminUserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IEnumerable<TbAdminUserDto>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<TbAdminUserDto> CreateAsync(CreateTbAdminUserDto createDto, CancellationToken cancellationToken = default);
    Task<TbAdminUserDto> UpdateAsync(long id, UpdateTbAdminUserDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken = default);
}
