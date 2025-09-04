using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.GdsV3;
using System.Reflection;

namespace TigerBooking.Infrastructure.Data;

public class GdsV3DbContext : BaseDbContext
{
    public DbSet<GdsV3User> Users { get; set; }

    public GdsV3DbContext(DbContextOptions<GdsV3DbContext> options) 
        : base(options, "gds_v3")
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 모든 Foreign Key를 Restrict로 설정
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // GdsV3 스키마의 Configuration 클래스들만 자동 적용
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            type => type.Namespace?.Contains("Data.Configurations.GdsV3") == true);
    }
}
