namespace PlayStation.Application.DTOs.Dashboard;

public class DashboardDto
{
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ActiveSessions { get; set; }
    public int TotalDevices { get; set; }
    public int AvailableDevices { get; set; }
    public int OccupiedDevices { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TodayExpenses { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public List<DeviceStatusSummaryDto> DeviceSummaries { get; set; } = new();
    public RevenueChartData RevenueChart { get; set; } = new();
    public ExpenseChartData ExpenseChart { get; set; } = new();
    public DeviceUsageChartData DeviceUsageChart { get; set; } = new();
    public SessionChartData SessionChart { get; set; } = new();
}

public class DeviceStatusSummaryDto
{
    public string DeviceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public int ActiveMinutes { get; set; }
}

public class RevenueChartData
{
    public List<DailyMetric> DailyRevenue { get; set; } = new();
    public List<DailyMetric> DailyExpenses { get; set; } = new();
    public List<DailyMetric> DailyProfit { get; set; } = new();
}

public class ExpenseChartData
{
    public List<CategoryMetric> ByCategory { get; set; } = new();
    public List<DailyMetric> DailyExpenses { get; set; } = new();
}

public class DeviceUsageChartData
{
    public List<DeviceUsageMetric> UsageByDevice { get; set; } = new();
    public List<StatusDistributionMetric> StatusDistribution { get; set; } = new();
}

public class SessionChartData
{
    public List<HourlyMetric> SessionsByHour { get; set; } = new();
    public List<DailyMetric> DailySessions { get; set; } = new();
    public List<DeviceSessionMetric> TopDevices { get; set; } = new();
}

public class DailyMetric
{
    public string Date { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class CategoryMetric
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class DeviceUsageMetric
{
    public string DeviceName { get; set; } = string.Empty;
    public decimal UsageHours { get; set; }
    public decimal Revenue { get; set; }
}

public class StatusDistributionMetric
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class HourlyMetric
{
    public int Hour { get; set; }
    public int SessionCount { get; set; }
}

public class DeviceSessionMetric
{
    public string DeviceName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public decimal Revenue { get; set; }
}
