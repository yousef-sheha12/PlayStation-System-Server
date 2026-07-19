using PlayStation.Domain.Enums;

namespace PlayStation.Application.DTOs.Device;

public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal HourlyRate { get; set; }
    public DeviceStatus Status { get; set; }
    public int ActiveSessionCount { get; set; }
}

public class CreateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal HourlyRate { get; set; }
}

public class UpdateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal HourlyRate { get; set; }
    public DeviceStatus Status { get; set; }
}
