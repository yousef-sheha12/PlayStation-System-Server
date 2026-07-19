using PlayStation.Domain.Enums;

namespace PlayStation.Application.DTOs.Invoice;

public class InvoiceDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class GenerateInvoiceDto
{
    public int SessionId { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}
