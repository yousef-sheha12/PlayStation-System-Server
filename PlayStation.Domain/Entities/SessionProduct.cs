using PlayStation.Domain.Common;

namespace PlayStation.Domain.Entities;

public class SessionProduct : BaseEntity
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
