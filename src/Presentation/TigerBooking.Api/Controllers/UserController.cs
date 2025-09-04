using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TigerBooking.Application.DTOs.Users;
using TigerBooking.Application.Interfaces;

namespace TigerBooking.Api.Controllers;

/// <summary>
/// 사용자 프로필 관련 API 컨트롤러
/// 인증된 사용자의 정보 조회 및 수정 기능을 제공
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// 내 정보 조회
    /// 현재 로그인한 사용자의 프로필 정보를 반환
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>사용자 프로필 정보</returns>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "인증 정보가 유효하지 않습니다." });
            }

            var profile = await _userService.GetMyProfileAsync(userId.Value, cancellationToken);
            if (profile == null)
            {
                return NotFound(new { message = "사용자를 찾을 수 없습니다." });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "사용자 프로필 조회 중 오류 발생");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 프로필 수정
    /// 현재 로그인한 사용자의 프로필 정보를 수정
    /// </summary>
    /// <param name="request">수정할 프로필 정보</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>수정된 사용자 프로필 정보</returns>
    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "인증 정보가 유효하지 않습니다." });
            }

            var updatedProfile = await _userService.UpdateProfileAsync(userId.Value, request, cancellationToken);
            _logger.LogInformation("사용자 프로필 수정 성공: {UserId}", userId.Value);
            return Ok(updatedProfile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("프로필 수정 실패: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "프로필 수정 중 오류 발생");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// 계정 삭제
    /// 현재 로그인한 사용자의 계정을 소프트 삭제
    /// </summary>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>삭제 결과</returns>
    [HttpDelete("me")]
    public async Task<ActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "인증 정보가 유효하지 않습니다." });
            }

            await _userService.DeleteAccountAsync(userId.Value, cancellationToken);
            _logger.LogInformation("사용자 계정 삭제 성공: {UserId}", userId.Value);
            return Ok(new { message = "계정이 성공적으로 삭제되었습니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "계정 삭제 중 오류 발생");
            return StatusCode(500, new { message = "서버 오류가 발생했습니다." });
        }
    }

    /// <summary>
    /// JWT 토큰에서 현재 사용자 ID를 추출
    /// </summary>
    /// <returns>사용자 ID 또는 null</returns>
    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value;
        
        if (long.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}
