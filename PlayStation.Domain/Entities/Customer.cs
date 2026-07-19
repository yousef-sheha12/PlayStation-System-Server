using PlayStation.Domain.Common;

namespace PlayStation.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
