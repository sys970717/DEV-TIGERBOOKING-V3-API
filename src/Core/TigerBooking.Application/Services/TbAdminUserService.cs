using TigerBooking.Application.DTOs.TbAdmin;
using TigerBooking.Application.Interfaces;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Application.Services;

/// <summary>
/// TbAdmin 스키마의 관리자 사용자 관리를 위한 서비스 구현체입니다.
/// 관리자 계정 관련 비즈니스 로직과 데이터 변환을 담당합니다.
/// </summary>
public class TbAdminUserService : ITbAdminUserService
{
    private readonly ITbAdminUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TbAdminUserService(ITbAdminUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TbAdminUserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<TbAdminUserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<TbAdminUserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<IEnumerable<TbAdminUserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<TbAdminUserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByRoleAsync(role, cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<TbAdminUserDto>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByDepartmentAsync(department, cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<TbAdminUserDto> CreateAsync(CreateTbAdminUserDto createDto, CancellationToken cancellationToken = default)
    {
        // 중복 체크
        if (await _userRepository.IsUsernameExistsAsync(createDto.Username, cancellationToken))
            throw new ArgumentException($"Username '{createDto.Username}' already exists.");

        if (await _userRepository.IsEmailExistsAsync(createDto.Email, cancellationToken))
            throw new ArgumentException($"Email '{createDto.Email}' already exists.");

        var user = new TbAdminUser
        {
            Username = createDto.Username,
            Email = createDto.Email,
            FullName = createDto.FullName,
            PasswordHash = HashPassword(createDto.Password), // TODO: 실제 해싱 구현
            Role = createDto.Role,
            Department = createDto.Department,
            IsActive = createDto.IsActive
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<TbAdminUserDto> UpdateAsync(long id, UpdateTbAdminUserDto updateDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new ArgumentException($"User with ID '{id}' not found.");

        // 이메일 중복 체크 (자신 제외)
        var existingUser = await _userRepository.GetByEmailAsync(updateDto.Email, cancellationToken);
        if (existingUser != null && existingUser.Id != id)
            throw new ArgumentException($"Email '{updateDto.Email}' already exists.");

        user.Email = updateDto.Email;
        user.FullName = updateDto.FullName;
        user.Role = updateDto.Role;
        user.Department = updateDto.Department;
        user.IsActive = updateDto.IsActive;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.IsUsernameExistsAsync(username, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.IsEmailExistsAsync(email, cancellationToken);
    }

    public async Task<bool> ValidatePasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        return VerifyPassword(password, user.PasswordHash); // TODO: 실제 검증 구현
    }

    private static TbAdminUserDto MapToDto(TbAdminUser user)
    {
        return new TbAdminUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Role = user.Role,
            Department = user.Department,
            CreatedAt = user.CreatedTz,
            UpdatedAt = user.UpdatedTz,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }

    private static string HashPassword(string password)
    {
        // TODO: 실제 BCrypt 또는 다른 해싱 알고리즘 구현
        return password; // 임시
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        // TODO: 실제 패스워드 검증 구현
        return password == hashedPassword; // 임시
    }
}
