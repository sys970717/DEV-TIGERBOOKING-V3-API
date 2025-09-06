using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TigerBooking.Application.DTOs.Users;
using TigerBooking.Application.DTOs.Common;
using TigerBooking.Application.Interfaces;

namespace TigerBooking.Api.Controllers;

/// <summary>
/// 사용자 인증 관련 API 컨트롤러
/// 회원가입, 로그인, 로그아웃 등의 인증 기능을 제공
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 회원가입 (LOCAL 계정)
    /// </summary>
    /// <param name="request">회원가입 요청 정보</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>생성된 사용자 정보</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userService.RegisterAsync(request, cancellationToken);
            _logger.LogInformation("사용자 회원가입 성공: {UserId}", result.Id);
            var response = ApiResponse<RegisterResponseDto>.Ok(result);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("회원가입 실패: {Message}", ex.Message);
            var err = new ErrorDetail { Code = "EMAIL_EXISTS", Message = ex.Message };
            return BadRequest(ApiResponse<RegisterResponseDto>.Fail(err));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회원가입 중 오류 발생");
            var err = new ErrorDetail { Code = "UNHANDLED_ERROR", Message = "서버 오류가 발생했습니다." };
            return StatusCode(500, ApiResponse<RegisterResponseDto>.Fail(err));
        }
    }

    /// <summary>
    /// 로그인 (LOCAL 계정)
    /// </summary>
    /// <param name="request">로그인 요청 정보</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>JWT 토큰 정보</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userService.LoginAsync(request, cancellationToken);
            _logger.LogInformation("사용자 로그인 성공: {Email}", request.Email);
            return Ok(ApiResponse<LoginResponseDto>.Ok(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("로그인 실패: {Email}, {Message}", request.Email, ex.Message);
            var err = new ErrorDetail { Code = "UNAUTHORIZED", Message = ex.Message };
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(err));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "로그인 중 오류 발생: {Email}", request.Email);
            var err = new ErrorDetail { Code = "UNHANDLED_ERROR", Message = "서버 오류가 발생했습니다." };
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail(err));
        }
    }

    /// <summary>
    /// Refresh token으로 새로운 access/refresh 토큰을 발급
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Refresh([
        FromBody] TigerBooking.Application.DTOs.Users.RefreshRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var tokenResp = await _userService.RefreshAsync(request.RefreshToken, cancellationToken);
            if (tokenResp == null)
            {
                var err = new ErrorDetail { Code = "INVALID_REFRESH", Message = "Refresh token이 유효하지 않습니다." };
                return Unauthorized(ApiResponse<TokenResponseDto>.Fail(err));
            }

            return Ok(ApiResponse<TokenResponseDto>.Ok(tokenResp));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh 처리 중 오류 발생");
            var err = new ErrorDetail { Code = "UNHANDLED_ERROR", Message = "서버 오류가 발생했습니다." };
            return StatusCode(500, ApiResponse<TokenResponseDto>.Fail(err));
        }
    }

    /// <summary>
    /// 로그아웃
    /// JWT 토큰을 무효화하여 로그아웃 처리
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>로그아웃 결과</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken cancellationToken)
    {
        try
        {
            // JWT에서 jti 클레임 추출
            var jti = User.FindFirst("jti")?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                var err = new ErrorDetail { Code = "INVALID_TOKEN", Message = "유효하지 않은 토큰입니다." };
                return BadRequest(ApiResponse<object>.Fail(err));
            }

            var result = await _userService.LogoutAsync(jti, cancellationToken);
            if (result)
            {
                _logger.LogInformation("사용자 로그아웃 성공: JTI {Jti}", jti);
                return Ok(ApiResponse<object>.Ok(new { ok = true }));
            }
            else
            {
                var err = new ErrorDetail { Code = "LOGOUT_FAILED", Message = "로그아웃에 실패했습니다." };
                return BadRequest(ApiResponse<object>.Fail(err));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "로그아웃 중 오류 발생");
            var err = new ErrorDetail { Code = "UNHANDLED_ERROR", Message = "서버 오류가 발생했습니다." };
            return StatusCode(500, ApiResponse<object>.Fail(err));
        }
    }
}
