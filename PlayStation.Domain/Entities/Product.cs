using PlayStation.Domain.Common;

namespace PlayStation.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<SessionProduct> SessionProducts { get; set; } = new List<SessionProduct>();
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
