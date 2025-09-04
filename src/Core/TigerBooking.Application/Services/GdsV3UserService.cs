using TigerBooking.Application.DTOs.GdsV3;
using TigerBooking.Application.Interfaces;
using TigerBooking.Domain.Entities.GdsV3;
using TigerBooking.Domain.Interfaces.GdsV3;
using TigerBooking.Domain.Common.Interfaces;

namespace TigerBooking.Application.Services;

/// <summary>
/// GdsV3 스키마의 일반 사용자 관리를 위한 서비스 구현체입니다.
/// 일반 사용자 계정 관련 비즈니스 로직과 데이터 변환을 담당합니다.
/// </summary>
public class GdsV3UserService : IGdsV3UserService
{
    private readonly IGdsV3UserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GdsV3UserService(IGdsV3UserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GdsV3UserDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<GdsV3UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<GdsV3UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<IEnumerable<GdsV3UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<GdsV3UserDto>> GetByCustomerTypeAsync(string customerType, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByCustomerTypeAsync(customerType, cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<GdsV3UserDto>> GetRecentlyLoggedInUsersAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetRecentlyLoggedInUsersAsync(since, cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<GdsV3UserDto>> GetUsersWithMultipleLoginAttemptsAsync(int minAttempts, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetUsersWithMultipleLoginAttemptsAsync(minAttempts, cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<GdsV3UserDto> CreateAsync(CreateGdsV3UserDto createDto, CancellationToken cancellationToken = default)
    {
        // 중복 체크
        if (await _userRepository.IsUsernameExistsAsync(createDto.Username, cancellationToken))
            throw new ArgumentException($"Username '{createDto.Username}' already exists.");

        if (await _userRepository.IsEmailExistsAsync(createDto.Email, cancellationToken))
            throw new ArgumentException($"Email '{createDto.Email}' already exists.");

        var user = new GdsV3User
        {
            Username = createDto.Username,
            Email = createDto.Email,
            FullName = createDto.FullName,
            PasswordHash = HashPassword(createDto.Password), // TODO: 실제 해싱 구현
            CustomerType = createDto.CustomerType,
            PhoneNumber = createDto.PhoneNumber,
            IsActive = createDto.IsActive,
            LoginAttempts = 0
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<GdsV3UserDto> UpdateAsync(long id, UpdateGdsV3UserDto updateDto, CancellationToken cancellationToken = default)
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
        user.CustomerType = updateDto.CustomerType;
        user.PhoneNumber = updateDto.PhoneNumber;
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

    public async Task HandleLoginSuccessAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateLastLoginAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleLoginFailureAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.IncrementLoginAttemptsAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetLoginAttemptsAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.ResetLoginAttemptsAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static GdsV3UserDto MapToDto(GdsV3User user)
    {
        return new GdsV3UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CustomerType = user.CustomerType,
            PhoneNumber = user.PhoneNumber,
            LastLoginAt = user.LastLoginAt,
            LoginAttempts = user.LoginAttempts,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
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
