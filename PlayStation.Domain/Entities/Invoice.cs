using PlayStation.Domain.Common;
using PlayStation.Domain.Enums;

namespace PlayStation.Domain.Entities;

public class Invoice : BaseEntity
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
