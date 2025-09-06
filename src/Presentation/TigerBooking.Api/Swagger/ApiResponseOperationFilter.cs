using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace TigerBooking.Api.Swagger;

public class ApiResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Skip if no responses
        if (operation.Responses == null) return;

        // For 200/201 responses, wrap schema into ApiResponse<T>
        foreach (var status in operation.Responses.Keys.ToList())
        {
            if (status.StartsWith("2"))
            {
                var response = operation.Responses[status];
                // if content has application/json with schema, wrap it
                if (response.Content != null && response.Content.TryGetValue("application/json", out var media))
                {
                    var origSchema = media.Schema;
                    media.Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["code"] = new OpenApiSchema { Type = "integer", Example = OpenApiAnyFactory.CreateFromJson("0") },
                            ["success"] = new OpenApiSchema { Type = "boolean", Example = OpenApiAnyFactory.CreateFromJson("true") },
                            ["data"] = origSchema ?? new OpenApiSchema { Type = "object" },
                            ["error"] = new OpenApiSchema { Type = "object", Nullable = true },
                            ["traceId"] = new OpenApiSchema { Type = "string" },
                            ["timestamp"] = new OpenApiSchema { Type = "string", Format = "date-time" }
                        }
                    };
                }
            }
        }
    }
}
