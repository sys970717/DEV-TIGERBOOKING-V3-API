using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TigerBooking.Infrastructure.Data;
using TigerBooking.Infrastructure.Repositories;
using TigerBooking.Infrastructure.Repositories.TbAdmin;
using TigerBooking.Domain.Common.Interfaces;
using TigerBooking.Domain.Interfaces.TbAdmin;

namespace TigerBooking.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Infrastructure 레이어의 의존성을 등록합니다.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context 등록
        AddDatabaseContext(services, configuration);
        
        // Repository 자동 등록
        RegisterRepositories(services);
        
        // Infrastructure Services 자동 등록
        RegisterInfrastructureServices(services);

        return services;
    }

    private static void AddDatabaseContext(IServiceCollection services, IConfiguration configuration)
    {
        // TbAdmin 스키마 DbContext 등록
        var tbAdminConnectionString = configuration.GetConnectionString("TBAdmin.Application.ConnectionString");
        services.AddDbContext<TbAdminDbContext>(options =>
        {
            options.UseNpgsql(tbAdminConnectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });
        
        // GdsV3 스키마 DbContext 등록
        var gdsV3ConnectionString = configuration.GetConnectionString("GdsV3.Application.ConnectionString");
        services.AddDbContext<GdsV3DbContext>(options =>
        {
            options.UseNpgsql(gdsV3ConnectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });
        
        // 스키마별 UnitOfWork 등록
        services.AddScoped<ITbAdminUnitOfWork, TbAdminUnitOfWork>();
        services.AddScoped<IGdsV3UnitOfWork, GdsV3UnitOfWork>();
        
        // Application에서 사용하는 일반 IUnitOfWork도 등록 (TbAdmin을 기본으로 사용)
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ITbAdminUnitOfWork>());
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        
        // Repository 인터페이스와 구현체 자동 매핑
        var repositoryTypes = infrastructureAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Repository"))
            .ToList();

        foreach (var implementationType in repositoryTypes)
        {
            // 해당 구현체가 구현하는 인터페이스 찾기 (I{RepositoryName} 패턴)
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }

    private static void RegisterInfrastructureServices(IServiceCollection services)
    {
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        
        // Infrastructure Service 인터페이스와 구현체 자동 매핑
        var serviceTypes = infrastructureAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && 
                          type.Name.EndsWith("Service") && 
                          !type.Name.Contains("Repository"))
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
    }
}
