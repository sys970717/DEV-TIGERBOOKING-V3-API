using TigerBooking.Domain.Common.Interfaces;
using TigerBooking.Infrastructure.Data;

namespace TigerBooking.Infrastructure.Repositories;

/// <summary>
/// GdsV3 스키마 전용 UnitOfWork
/// </summary>
public interface IGdsV3UnitOfWork : IUnitOfWork
{
}

public class GdsV3UnitOfWork : UnitOfWork<GdsV3DbContext>, IGdsV3UnitOfWork
{
    public GdsV3UnitOfWork(GdsV3DbContext context) : base(context)
    {
    }
}
