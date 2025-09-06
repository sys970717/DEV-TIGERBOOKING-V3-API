using System.Text.Json;
using TigerBooking.Application.DTOs.Common;

namespace TigerBooking.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier ?? string.Empty;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var error = new ErrorDetail { Code = "UNHANDLED_ERROR", Message = "서버 오류가 발생했습니다." };
            var response = ApiResponse<object>.Fail(error, traceId, code: -1);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
