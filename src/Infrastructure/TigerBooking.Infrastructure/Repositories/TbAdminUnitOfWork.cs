using TigerBooking.Domain.Common.Interfaces;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// TbAdmin 스키마 전용 UnitOfWork
/// </summary>
public interface ITbAdminUnitOfWork : IUnitOfWork
{
}

public class TbAdminUnitOfWork : UnitOfWork<TbAdminDbContext>, ITbAdminUnitOfWork
{
    public TbAdminUnitOfWork(TbAdminDbContext context) : base(context)
    {
    }
}
