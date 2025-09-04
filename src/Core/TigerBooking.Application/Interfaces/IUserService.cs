using TigerBooking.Application.DTOs.Users;

namespace TigerBooking.Application.Interfaces;

/// <summary>
/// 사용자 서비스 인터페이스
/// B2C 고객 사용자 관련 비즈니스 로직을 담당
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 회원가입 (LOCAL 계정)
    /// 이메일 중복 검사, 비밀번호 해시, PII 암호화 등을 처리
    /// </summary>
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그인 (LOCAL 계정)
    /// 인증 후 JWT 토큰 발급 및 Redis에 허용 리스트 등록
    /// </summary>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 로그아웃
    /// Redis에서 토큰 무효화
    /// </summary>
    Task<bool> LogoutAsync(string jti, CancellationToken cancellationToken = default);

    /// <summary>
    /// 내 정보 조회
    /// 암호화된 데이터 복호화 후 반환
    /// </summary>
    Task<UserProfileDto?> GetMyProfileAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 프로필 업데이트
    /// </summary>
    Task<UserProfileDto> UpdateProfileAsync(long userId, RegisterRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 계정 삭제 (소프트 삭제)
    /// </summary>
    Task DeleteAccountAsync(long userId, CancellationToken cancellationToken = default);
}
