using PlayStation.Application.DTOs.Expense;

namespace PlayStation.Application.DTOs.Report;

public class DailyRevenueReportDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DeviceRevenue { get; set; }
    public decimal ProductRevenue { get; set; }
    public int TotalSessions { get; set; }
    public int TotalInvoices { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public List<HourlyRevenueDto> HourlyBreakdown { get; set; } = new();
}

public class HourlyRevenueDto
{
    public int Hour { get; set; }
    public decimal Revenue { get; set; }
    public int SessionCount { get; set; }
}

public class MonthlyRevenueReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal DeviceRevenue { get; set; }
    public decimal ProductRevenue { get; set; }
    public int TotalSessions { get; set; }
    public decimal AverageDailyRevenue { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int SessionCount { get; set; }
}

public class YearlyRevenueReportDto
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DeviceRevenue { get; set; }
    public decimal ProductRevenue { get; set; }
    public int TotalSessions { get; set; }
    public decimal AverageMonthlyRevenue { get; set; }
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int SessionCount { get; set; }
}

public class MostUsedDeviceDto
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class MostSoldProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ExpensesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalExpenses { get; set; }
    public List<ExpenseCategoryDto> CategoryBreakdown { get; set; } = new();
    public List<ExpenseDto> Expenses { get; set; } = new();
}

public class ExpenseCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class ProfitReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public List<DailyProfitDto> DailyBreakdown { get; set; } = new();
}

public class DailyProfitDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
}
