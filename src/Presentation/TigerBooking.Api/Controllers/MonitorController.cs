using Microsoft.AspNetCore.Mvc;
using TigerBooking.Infrastructure.Services.Redis;

namespace TigerBooking.Api.Controllers;

[ApiController]
[Route("monitor")]
public class MonitorController : ControllerBase
{
    private readonly IRedisClient _redisClient;

    public MonitorController(IRedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    /// <summary>
    /// 캐시 상태 확인
    /// </summary>
    /// <returns>Redis 연결 상태 및 샘플 통계</returns>
    [HttpGet("cache")]
    public async Task<IActionResult> GetCacheStatus(CancellationToken cancellationToken)
    {
        try
        {
            // 연결 확인: 키 존재 여부 검사와 샘플 키 목록 조회
            var sampleKeys = await _redisClient.GetKeysAsync("*");
            var keyList = sampleKeys?.Take(50).ToList() ?? new List<string>();
            var keyCount = keyList.Count;

            var response = new
            {
                ok = true,
                provider = "redis",
                sampleKeyCount = keyCount,
                sampleKeys = keyList
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new
            {
                ok = false,
                provider = "redis",
                error = ex.Message
            };
            return StatusCode(503, response);
        }
    }
}
