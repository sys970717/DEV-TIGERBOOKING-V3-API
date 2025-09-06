using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using TigerBooking.Application.Configuration;
using TigerBooking.Application.Extensions;
using TigerBooking.Application.Interfaces;
using TigerBooking.Infrastructure.Extensions;
using TigerBooking.Infrastructure.Services;
using TigerBooking.Api.Middleware;
using TigerBooking.Api.Filters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Configuration 바인딩
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection(RedisSettings.SectionName));

// Redis 연결 또는 InMemory 대체
var redisSettings = builder.Configuration.GetSection(TigerBooking.Application.Configuration.RedisSettings.SectionName)
    .Get<TigerBooking.Application.Configuration.RedisSettings>() ?? new TigerBooking.Application.Configuration.RedisSettings();
if (redisSettings.UseRedis)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis")
                              ?? throw new InvalidOperationException("Redis connection string not found.");
        return ConnectionMultiplexer.Connect(connectionString);
    });
    // Register Redis client wrapper
    builder.Services.AddSingleton<TigerBooking.Infrastructure.Services.Redis.IRedisClient>(provider =>
    {
        var mux = provider.GetRequiredService<IConnectionMultiplexer>();
        return new TigerBooking.Infrastructure.Services.Redis.RedisClientWrapper(mux);
    });
}
else
{
    // In-memory fake Redis implementation for local/dev environments
    builder.Services.AddSingleton<TigerBooking.Infrastructure.Services.Redis.IRedisClient, TigerBooking.Infrastructure.Services.Redis.InMemoryRedisClient>();
}

// JWT 인증 설정
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                 ?? throw new InvalidOperationException("JWT settings not found.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Redis에서 토큰 유효성 추가 검증
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                
                var isValid = await tokenService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    context.Fail("Token is not active in Redis");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Application 및 Infrastructure 서비스 등록
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 토큰 서비스 등록
builder.Services.AddScoped<ITokenService, TokenService>();

// Slack notifier (skeleton) - will send webhook if configured
builder.Services.AddHttpClient<TigerBooking.Api.Services.ISlackNotifier, TigerBooking.Api.Services.SlackNotifier>();

// Controllers 등록
builder.Services.AddControllers(options =>
    {
        // 전역 응답 래핑 필터 등록
        options.Filters.Add<ResponseWrappingFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // ModelState invalid 시 ApiResponse<...> 형태로 반환
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kv => kv.Value != null && kv.Value.Errors != null && kv.Value.Errors.Count > 0)
                .Select(kv => new { Field = kv.Key, Message = kv.Value!.Errors.First().ErrorMessage })
                .ToList();

            var first = errors.FirstOrDefault();
            var detail = new TigerBooking.Application.DTOs.Common.ErrorDetail
            {
                Code = "VALIDATION_ERROR",
                Message = first?.Message ?? "Validation failed",
                Details = errors
            };

            var apiResp = TigerBooking.Application.DTOs.Common.ApiResponse<object>.Fail(detail, context.HttpContext.TraceIdentifier ?? string.Empty, code: 400);

            return new BadRequestObjectResult(apiResp);
        };
        // API 응답을 camelCase로 변환
        options.SuppressModelStateInvalidFilter = false;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TigerBooking B2C API", Version = "v1" });
    
    // JWT Bearer 인증 설정 추가
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    // ApiResponse wrapper operation filter
    c.OperationFilter<TigerBooking.Api.Swagger.ApiResponseOperationFilter>();
});

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseHsts();
app.UseHttpsRedirection();

// 전역 예외 처리 미들웨어 등록(가장 먼저)
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TigerBooking B2C API V1");
    c.RoutePrefix = string.Empty; // Swagger를 루트 경로에서 접근 가능하도록 설정
});

app.UseCors();

// 인증/인가 미들웨어 순서 중요
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
