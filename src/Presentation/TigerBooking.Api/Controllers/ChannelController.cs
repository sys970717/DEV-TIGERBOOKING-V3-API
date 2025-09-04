using Microsoft.AspNetCore.Mvc;
using TigerBooking.Application.DTOs.TbAdmin;
using TigerBooking.Application.Interfaces;

namespace TigerBooking.Api.Controllers;

/// <summary>
/// 채널 관리 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/admin/channels")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    /// <summary>
    /// 채널 목록 조회
    /// </summary>
    /// <param name="request">검색 및 페이징 조건</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>채널 목록</returns>
    [HttpGet]
    public async Task<ActionResult<GetChannelsResponseDto>> GetChannels(
        [FromQuery] GetChannelsRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _channelService.GetChannelsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// 채널 상세 조회
    /// </summary>
    /// <param name="id">채널 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>채널 상세 정보</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ChannelDto>> GetChannel(
        long id,
        CancellationToken cancellationToken)
    {
        var channel = await _channelService.GetChannelByIdAsync(id, cancellationToken);
        if (channel == null)
        {
            return NotFound($"채널을 찾을 수 없습니다. ID: {id}");
        }

        return Ok(channel);
    }

    /// <summary>
    /// 채널 생성
    /// </summary>
    /// <param name="request">채널 생성 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>생성된 채널 정보</returns>
    [HttpPost]
    public async Task<ActionResult<ChannelDto>> CreateChannel(
        CreateChannelRequestDto request,
        CancellationToken cancellationToken)
    {
        // TODO: JWT 토큰에서 사용자 정보 추출
        var createdBy = "system"; // 임시값

        var channel = await _channelService.CreateChannelAsync(request, createdBy, cancellationToken);
        return CreatedAtAction(nameof(GetChannel), new { id = channel.Id }, channel);
    }

    /// <summary>
    /// 채널 수정
    /// </summary>
    /// <param name="id">채널 ID</param>
    /// <param name="request">채널 수정 요청</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>수정된 채널 정보</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ChannelDto>> UpdateChannel(
        long id,
        UpdateChannelRequestDto request,
        CancellationToken cancellationToken)
    {
        // TODO: JWT 토큰에서 사용자 정보 추출
        var updatedBy = "system"; // 임시값

        var channel = await _channelService.UpdateChannelAsync(id, request, updatedBy, cancellationToken);
        return Ok(channel);
    }

    /// <summary>
    /// 채널 삭제 (소프트 삭제)
    /// </summary>
    /// <param name="id">채널 ID</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>삭제 결과</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteChannel(
        long id,
        CancellationToken cancellationToken)
    {
        // TODO: JWT 토큰에서 사용자 정보 추출
        var deletedBy = "system"; // 임시값

        await _channelService.DeleteChannelAsync(id, deletedBy, cancellationToken);
        return NoContent();
    }
}
