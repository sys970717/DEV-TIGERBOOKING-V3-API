namespace TigerBooking.Application.DTOs.Common;

/// <summary>
/// 구조화된 에러 정보
/// </summary>
public class ErrorDetail
{
    public string? Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}

/// <summary>
/// 공통 API 응답 포맷
/// code: 내부 비즈니스 코드(0: 성공, 음수/양수는 서비스 정의)
/// success: 네트워크/로직 결과
/// data: 실제 응답 페이로드
/// error: 내부 실패 메시지(네트워크는 성공이나 비즈니스 실패 시 포함)
/// traceId, timestamp 추가
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorDetail? Error { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string traceId = "", int code = 0) => new ApiResponse<T>
    {
        Code = code,
        Success = true,
        Data = data,
        Error = null,
        TraceId = traceId,
        Timestamp = DateTime.UtcNow
    };

    public static ApiResponse<T> Fail(ErrorDetail error, string traceId = "", int code = -1) => new ApiResponse<T>
    {
        Code = code,
        Success = false,
        Data = default,
        Error = error,
        TraceId = traceId,
        Timestamp = DateTime.UtcNow
    };
}

