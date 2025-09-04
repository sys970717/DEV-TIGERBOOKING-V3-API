using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TigerBooking.Application.DTOs.Users;
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
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userService.RegisterAsync(request, cancellationToken);
            _logger.LogInformation("사용자 회원가입 성공: {UserId}", result.Id);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("회원가입 실패: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "회원가입 중 오류 발생");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 로그인 (LOCAL 계정)
    /// </summary>
    /// <param name="request">로그인 요청 정보</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>JWT 토큰 정보</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _userService.LoginAsync(request, cancellationToken);
            _logger.LogInformation("사용자 로그인 성공: {Email}", request.Email);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("로그인 실패: {Email}, {Message}", request.Email, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "로그인 중 오류 발생: {Email}", request.Email);
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
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
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            // JWT에서 jti 클레임 추출
            var jti = User.FindFirst("jti")?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                return BadRequest(new { message = "유효하지 않은 토큰입니다." });
            }

            var result = await _userService.LogoutAsync(jti, cancellationToken);
            if (result)
            {
                _logger.LogInformation("사용자 로그아웃 성공: JTI {Jti}", jti);
                return Ok(new { ok = true });
            }
            else
            {
                return BadRequest(new { message = "로그아웃에 실패했습니다." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "로그아웃 중 오류 발생");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }
}
