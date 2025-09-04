using Microsoft.Extensions.Configuration;
using TigerBooking.Application.DTOs.Users;
using TigerBooking.Application.Interfaces;
using TigerBooking.Domain.Entities.TbAdmin;
using TigerBooking.Domain.Interfaces.TbAdmin;
using BCrypt.Net;

namespace TigerBooking.Application.Services;

/// <summary>
/// 사용자 서비스 구현체
/// B2C 고객 사용자 관련 비즈니스 로직 처리
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ISocialAuthRepository _socialAuthRepository;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    
    // 현재는 채널 ID를 1로 고정 (향후 환경설정으로 변경 예정)
    private const long DEFAULT_CHANNEL_ID = 1;

    public UserService(
        IUserRepository userRepository,
        ISocialAuthRepository socialAuthRepository,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _socialAuthRepository = socialAuthRepository;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // 이메일 정규화 및 중복 검사
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userRepository.GetByEmailAndChannelAsync(normalizedEmail, DEFAULT_CHANNEL_ID, cancellationToken);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("이미 존재하는 이메일입니다.");
        }

        // 회원가입하려는 채널이 활성 상태인지 확인 (Channel 테이블에서 직접 확인하는 로직 필요)
        // TODO: Channel 존재 여부 및 활성화 상태 확인 로직 추가

        // 비밀번호 해시 생성
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 전화번호 정규화 (숫자만 남기기)
        var normalizedPhoneNumber = !string.IsNullOrEmpty(request.PhoneNumber) 
            ? NormalizePhoneNumber(request.PhoneNumber) 
            : null;

        // 새 사용자 엔티티 생성 (PII 암호화는 향후 구현)
        var user = new User
        {
            ChannelId = DEFAULT_CHANNEL_ID,
            Email = EncryptPII(normalizedEmail), // 향후 암호화 구현
            Password = passwordHash,
            FamilyName = EncryptPII(request.FamilyName),
            GivenName = EncryptPII(request.GivenName),
            Gender = !string.IsNullOrEmpty(request.Gender) ? EncryptPII(request.Gender) : null,
            Nickname = !string.IsNullOrEmpty(request.Nickname) ? EncryptPII(request.Nickname) : null,
            PhoneCountryCode = !string.IsNullOrEmpty(request.PhoneCountryCode) ? EncryptPII(request.PhoneCountryCode) : null,
            PhoneNumber = !string.IsNullOrEmpty(normalizedPhoneNumber) ? EncryptPII(normalizedPhoneNumber) : null,
            NationalityCode = !string.IsNullOrEmpty(request.NationalityCode) ? EncryptPII(request.NationalityCode) : null,
            Point = 0,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        return new RegisterResponseDto
        {
            Id = createdUser.Id,
            ChannelId = createdUser.ChannelId
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // 이메일 정규화 및 활성 채널을 가진 사용자 조회
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAndActiveChannelAsync(normalizedEmail, DEFAULT_CHANNEL_ID, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.Password))
        {
            throw new UnauthorizedAccessException("이메일 또는 비밀번호가 올바르지 않거나 채널이 비활성화되었습니다.");
        }

        // 계정 잠금 확인
        if (user.LockedUntilTz.HasValue && user.LockedUntilTz.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("계정이 일시적으로 잠겨있습니다.");
        }

        // 비밀번호 검증
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            // 로그인 실패 횟수 증가
            await _userRepository.IncrementFailedLoginCountAsync(user.Id, cancellationToken);
            
            // 실패 횟수가 5회 이상이면 계정 잠금 (30분)
            if (user.FailedLoginCount >= 4) // 4회 실패 후 5회째에서 잠금
            {
                await _userRepository.LockAccountAsync(user.Id, DateTime.UtcNow.AddMinutes(30), cancellationToken);
            }
            
            throw new UnauthorizedAccessException("이메일 또는 비밀번호가 올바르지 않거나 채널이 비활성화되었습니다.");
        }

        // 로그인 성공 처리
        await _userRepository.ResetFailedLoginCountAsync(user.Id, cancellationToken);
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        // JWT 토큰 발급 및 Redis 등록
        var tokenResponse = await _tokenService.GenerateTokenAsync(user.Id, user.ChannelId, cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = tokenResponse.AccessToken,
            TokenType = "Bearer",
            ExpiresIn = tokenResponse.ExpiresIn,
            Jti = tokenResponse.Jti
        };
    }

    public async Task<bool> LogoutAsync(string jti, CancellationToken cancellationToken = default)
    {
        return await _tokenService.RevokeTokenAsync(jti, cancellationToken);
    }

    public async Task<UserProfileDto?> GetMyProfileAsync(long userId, CancellationToken cancellationToken = default)
    {
        // 활성 채널을 가진 사용자만 조회
        var user = await _userRepository.GetByIdWithActiveChannelAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            Id = user.Id,
            ChannelId = user.ChannelId,
            Email = DecryptPII(user.Email), // 향후 복호화 구현
            FamilyName = DecryptPII(user.FamilyName),
            GivenName = DecryptPII(user.GivenName),
            Gender = !string.IsNullOrEmpty(user.Gender) ? DecryptPII(user.Gender) : null,
            Nickname = !string.IsNullOrEmpty(user.Nickname) ? DecryptPII(user.Nickname) : null,
            PhoneCountryCode = !string.IsNullOrEmpty(user.PhoneCountryCode) ? DecryptPII(user.PhoneCountryCode) : null,
            PhoneNumber = !string.IsNullOrEmpty(user.PhoneNumber) ? DecryptPII(user.PhoneNumber) : null,
            NationalityCode = !string.IsNullOrEmpty(user.NationalityCode) ? DecryptPII(user.NationalityCode) : null,
            Point = user.Point,
            IsActive = user.IsActive
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(long userId, RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        // 활성 채널을 가진 사용자만 프로필 수정 가능
        var user = await _userRepository.GetByIdWithActiveChannelAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("사용자를 찾을 수 없거나 채널이 비활성화되었습니다.");
        }

        // 전화번호 정규화 (숫자만 남기기)
        var normalizedPhoneNumber = !string.IsNullOrEmpty(request.PhoneNumber) 
            ? NormalizePhoneNumber(request.PhoneNumber) 
            : null;

        // 프로필 정보 업데이트
        user.FamilyName = EncryptPII(request.FamilyName);
        user.GivenName = EncryptPII(request.GivenName);
        user.Gender = !string.IsNullOrEmpty(request.Gender) ? EncryptPII(request.Gender) : null;
        user.Nickname = !string.IsNullOrEmpty(request.Nickname) ? EncryptPII(request.Nickname) : null;
        user.PhoneCountryCode = !string.IsNullOrEmpty(request.PhoneCountryCode) ? EncryptPII(request.PhoneCountryCode) : null;
        user.PhoneNumber = !string.IsNullOrEmpty(normalizedPhoneNumber) ? EncryptPII(normalizedPhoneNumber) : null;
        user.NationalityCode = !string.IsNullOrEmpty(request.NationalityCode) ? EncryptPII(request.NationalityCode) : null;

        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserProfileDto
        {
            Id = updatedUser.Id,
            ChannelId = updatedUser.ChannelId,
            Email = DecryptPII(updatedUser.Email),
            FamilyName = DecryptPII(updatedUser.FamilyName),
            GivenName = DecryptPII(updatedUser.GivenName),
            Gender = !string.IsNullOrEmpty(updatedUser.Gender) ? DecryptPII(updatedUser.Gender) : null,
            Nickname = !string.IsNullOrEmpty(updatedUser.Nickname) ? DecryptPII(updatedUser.Nickname) : null,
            PhoneCountryCode = !string.IsNullOrEmpty(updatedUser.PhoneCountryCode) ? DecryptPII(updatedUser.PhoneCountryCode) : null,
            PhoneNumber = !string.IsNullOrEmpty(updatedUser.PhoneNumber) ? DecryptPII(updatedUser.PhoneNumber) : null,
            NationalityCode = !string.IsNullOrEmpty(updatedUser.NationalityCode) ? DecryptPII(updatedUser.NationalityCode) : null,
            Point = updatedUser.Point,
            IsActive = updatedUser.IsActive
        };
    }

    public async Task DeleteAccountAsync(long userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(userId, cancellationToken);
    }

    // Private helper methods
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // 전화번호에서 숫자만 남기기
        return new string(phoneNumber.Where(char.IsDigit).ToArray());
    }

    // TODO: 실제 암호화 구현 필요
    private static string EncryptPII(string plainText)
    {
        // 현재는 임시로 그대로 반환 (향후 AES 암호화 구현)
        return plainText;
    }

    // TODO: 실제 복호화 구현 필요  
    private static string DecryptPII(string encryptedText)
    {
        // 현재는 임시로 그대로 반환 (향후 AES 복호화 구현)
        return encryptedText;
    }
}
