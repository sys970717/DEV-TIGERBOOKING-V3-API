using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using TigerBooking.Application.Configuration;
using TigerBooking.Application.Extensions;
using TigerBooking.Application.Interfaces;
using TigerBooking.Infrastructure.Extensions;
using TigerBooking.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration 바인딩
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection(RedisSettings.SectionName));

// Redis 연결
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") 
                          ?? throw new InvalidOperationException("Redis connection string not found.");
    return ConnectionMultiplexer.Connect(connectionString);
});

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

// Controllers 등록
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TigerBooking B2C API V1");
    c.RoutePrefix = string.Empty; // Swagger를 루트 경로에서 접근 가능하도록 설정
});

app.UseHttpsRedirection();

app.UseCors();

// 인증/인가 미들웨어 순서 중요
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
