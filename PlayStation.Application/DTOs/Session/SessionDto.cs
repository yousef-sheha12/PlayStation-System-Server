using PlayStation.Domain.Enums;

namespace PlayStation.Application.DTOs.Session;

public class SessionDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? PauseTime { get; set; }
    public double? TotalPauseDurationSeconds { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal DeviceCost { get; set; }
    public decimal ProductsCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalCost { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SessionProductDto> Products { get; set; } = new();
    public List<SessionProductDto> SessionProducts { get; set; } = new();
}

public class SessionProductDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class StartSessionDto
{
    public int DeviceId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public decimal HourlyRate { get; set; }
}

public class AddProductToSessionDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class EndSessionDto
{
    public decimal Discount { get; set; }
}
