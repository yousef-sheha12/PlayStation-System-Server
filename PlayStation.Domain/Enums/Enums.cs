namespace PlayStation.Domain.Enums;

public enum DeviceStatus
{
    Available = 0,
    Occupied = 1,
    Maintenance = 2
}

public enum SessionStatus
{
    Active = 0,
    Paused = 1,
    Ended = 2
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    MobilePayment = 2
}
