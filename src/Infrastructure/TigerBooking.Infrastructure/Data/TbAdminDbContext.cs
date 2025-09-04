using Microsoft.EntityFrameworkCore;
using TigerBooking.Domain.Entities.TbAdmin;
using System.Reflection;

namespace TigerBooking.Infrastructure.Data;

public class TbAdminDbContext : BaseDbContext
{
    public DbSet<TbAdminUser> AdminUsers { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SocialAuth> SocialAuths { get; set; }

    public TbAdminDbContext(DbContextOptions<TbAdminDbContext> options) 
        : base(options, "tb_admin")
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

        // TbAdmin 스키마의 Configuration 클래스들만 자동 적용
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            type => type.Namespace?.Contains("Data.Configurations.TbAdmin") == true);
    }
}
