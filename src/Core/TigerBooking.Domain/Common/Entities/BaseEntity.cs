namespace TigerBooking.Domain.Common.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }
    public DateTime CreatedTz { get; set; }
    public DateTime UpdatedTz { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedTz { get; set; }
    public string? DeletedBy { get; set; }
}
