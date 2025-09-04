using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Domain.Interfaces.TbAdmin;

/// <summary>
/// TbAdmin 스키마의 사용자 관리를 위한 Repository 인터페이스입니다.
/// 관리자 계정 관련 특화된 기능을 제공합니다.
/// </summary>
public interface ITbAdminUserRepository : IRepository<TbAdminUser>
{
    Task<TbAdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<TbAdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<TbAdminUser>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IEnumerable<TbAdminUser>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
}
