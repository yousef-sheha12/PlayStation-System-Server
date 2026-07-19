using PlayStation.Domain.Common;
using PlayStation.Domain.Enums;

namespace PlayStation.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal HourlyRate { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Available;
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
