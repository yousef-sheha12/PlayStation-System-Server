using PlayStation.Domain.Common;
using PlayStation.Domain.Enums;

namespace PlayStation.Domain.Entities;

public class Session : BaseEntity
{
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? PauseTime { get; set; }
    public TimeSpan? TotalPauseDuration { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal DeviceCost { get; set; }
    public decimal ProductsCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalCost { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public Invoice? Invoice { get; set; }
    public ICollection<SessionProduct> SessionProducts { get; set; } = new List<SessionProduct>();
}
