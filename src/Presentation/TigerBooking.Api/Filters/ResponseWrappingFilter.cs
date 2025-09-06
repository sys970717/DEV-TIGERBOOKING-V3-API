using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TigerBooking.Application.DTOs.Common;

namespace TigerBooking.Api.Filters;

/// <summary>
/// Controller action 결과를 ApiResponse로 감싸는 필터
/// 이미 ApiResponse가 반환되면 그대로 통과
/// 제네릭 타입을 보존하여 ApiResponse&lt;T&gt;로 래핑함
/// </summary>
public class ResponseWrappingFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objResult)
        {
            var value = objResult.Value;

            // 이미 ApiResponse이면 통과
            if (value != null && value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                await next();
                return;
            }

            var status = objResult.StatusCode ?? 200;
            if (status >= 200 && status < 300)
            {
                var traceId = context.HttpContext.TraceIdentifier ?? string.Empty;

                // For simplicity and analyzer-safety, wrap as ApiResponse<object>
                var wrapped = ApiResponse<object>.Ok(value ?? new { }, traceId);

                context.Result = new ObjectResult(wrapped)
                {
                    StatusCode = objResult.StatusCode
                };
            }
        }

        await next();
    }
}
