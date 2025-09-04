namespace TigerBooking.Application.Configuration;

/// <summary>
/// JWT 토큰 설정을 담는 클래스입니다.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}

/// <summary>
/// Redis 설정을 담는 클래스입니다.
/// </summary>
public class RedisSettings
{
    public const string SectionName = "RedisSettings";
    
    public string TokenKeyPrefix { get; set; } = "tiger:token:";
    public string UserSessionPrefix { get; set; } = "tiger:session:";
}
