using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TigerBooking.Application.Interfaces;
using TigerBooking.Application.Services;

namespace TigerBooking.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Application 레이어의 서비스들을 DI 컨테이너에 등록합니다.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application Services 자동 등록
        RegisterApplicationServices(services);
        
        return services;
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        var applicationAssembly = Assembly.GetExecutingAssembly();
        
        // Application Service 인터페이스와 구현체 자동 매핑
        var serviceTypes = applicationAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && 
                          type.Name.EndsWith("Service") && 
                          type.Namespace?.Contains("Services") == true)
            .ToList();

        foreach (var implementationType in serviceTypes)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        // 명시적 서비스 등록
        services.AddScoped<ITbAdminUserService, TbAdminUserService>();
        services.AddScoped<IGdsV3UserService, GdsV3UserService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IUserService, UserService>();
    }
}
