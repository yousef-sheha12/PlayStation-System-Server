using PlayStation.Application.DTOs.Report;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Enums;
using PlayStation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PlayStation.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly PlayStationDbContext _context;

    public ReportService(PlayStationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyRevenueReportDto> GetDailyRevenueAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var invoices = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue &&
                        i.PaidAt.Value >= startOfDay && i.PaidAt.Value < endOfDay)
            .Include(i => i.Session).ThenInclude(s => s.Device)
            .ToListAsync();

        var sessions = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime >= startOfDay && s.StartTime < endOfDay)
            .ToListAsync();

        var hourlyBreakdown = Enumerable.Range(0, 24).Select(hour => new HourlyRevenueDto
        {
            Hour = hour,
            Revenue = invoices.Where(i => i.PaidAt!.Value.Hour == hour).Sum(i => i.TotalAmount),
            SessionCount = sessions.Count(s => s.StartTime.Hour == hour)
        }).ToList();

        return new DailyRevenueReportDto
        {
            Date = date,
            TotalRevenue = invoices.Sum(i => i.TotalAmount),
            DeviceRevenue = sessions.Sum(s => s.DeviceCost),
            ProductRevenue = sessions.Sum(s => s.ProductsCost),
            TotalSessions = sessions.Count,
            TotalInvoices = invoices.Count,
            AverageSessionDuration = sessions.Any() ? (decimal)sessions.Average(s => s.TotalHours) : 0,
            HourlyBreakdown = hourlyBreakdown
        };
    }

    public async Task<MonthlyRevenueReportDto> GetMonthlyRevenueAsync(int year, int month)
    {
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var invoices = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue &&
                        i.PaidAt.Value >= startOfMonth && i.PaidAt.Value < endOfMonth)
            .ToListAsync();

        var sessions = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime >= startOfMonth && s.StartTime < endOfMonth)
            .ToListAsync();

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyBreakdown = Enumerable.Range(1, daysInMonth).Select(day => new DailyRevenueDto
        {
            Date = new DateTime(year, month, day),
            Revenue = invoices.Where(i => i.PaidAt!.Value.Day == day).Sum(i => i.TotalAmount),
            SessionCount = sessions.Count(s => s.StartTime.Day == day)
        }).ToList();

        return new MonthlyRevenueReportDto
        {
            Year = year,
            Month = month,
            MonthName = startOfMonth.ToString("MMMM"),
            TotalRevenue = invoices.Sum(i => i.TotalAmount),
            DeviceRevenue = sessions.Sum(s => s.DeviceCost),
            ProductRevenue = sessions.Sum(s => s.ProductsCost),
            TotalSessions = sessions.Count,
            AverageDailyRevenue = daysInMonth > 0 ? invoices.Sum(i => i.TotalAmount) / daysInMonth : 0,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<YearlyRevenueReportDto> GetYearlyRevenueAsync(int year)
    {
        var startOfYear = new DateTime(year, 1, 1);
        var endOfYear = startOfYear.AddYears(1);

        var invoices = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue &&
                        i.PaidAt.Value >= startOfYear && i.PaidAt.Value < endOfYear)
            .ToListAsync();

        var sessions = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime >= startOfYear && s.StartTime < endOfYear)
            .ToListAsync();

        var monthlyBreakdown = Enumerable.Range(1, 12).Select(month => new MonthlyRevenueDto
        {
            Month = month,
            MonthName = new DateTime(year, month, 1).ToString("MMM"),
            Revenue = invoices.Where(i => i.PaidAt!.Value.Month == month).Sum(i => i.TotalAmount),
            SessionCount = sessions.Count(s => s.StartTime.Month == month)
        }).ToList();

        return new YearlyRevenueReportDto
        {
            Year = year,
            TotalRevenue = invoices.Sum(i => i.TotalAmount),
            DeviceRevenue = sessions.Sum(s => s.DeviceCost),
            ProductRevenue = sessions.Sum(s => s.ProductsCost),
            TotalSessions = sessions.Count,
            AverageMonthlyRevenue = invoices.Sum(i => i.TotalAmount) / 12,
            MonthlyBreakdown = monthlyBreakdown
        };
    }

    public async Task<List<MostUsedDeviceDto>> GetMostUsedDevicesAsync(int count = 5)
    {
        return await _context.Devices
            .Where(d => !d.IsDeleted)
            .Select(d => new MostUsedDeviceDto
            {
                DeviceId = d.Id,
                DeviceName = d.Name,
                TotalSessions = d.Sessions.Count(s => !s.IsDeleted),
                TotalHours = d.Sessions.Where(s => !s.IsDeleted).Sum(s => s.TotalHours),
                TotalRevenue = d.Sessions.Where(s => !s.IsDeleted).Sum(s => s.DeviceCost)
            })
            .OrderByDescending(d => d.TotalSessions)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<MostSoldProductDto>> GetMostSoldProductsAsync(int count = 5)
    {
        return await _context.SessionProducts
            .Where(sp => !sp.IsDeleted)
            .GroupBy(sp => new { sp.ProductId, sp.Product.Name, CategoryName = sp.Product.Category.Name })
            .Select(g => new MostSoldProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                TotalQuantitySold = g.Sum(sp => sp.Quantity),
                TotalRevenue = g.Sum(sp => sp.TotalPrice)
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ExpensesReportDto> GetExpensesReportAsync(DateTime startDate, DateTime endDate)
    {
        var endOfEndDate = endDate.Date.AddDays(1);

        var expenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate < endOfEndDate)
            .ToListAsync();

        var categoryBreakdown = expenses
            .GroupBy(e => e.Category ?? "Uncategorized")
            .Select(g => new ExpenseCategoryDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .ToList();

        return new ExpensesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalExpenses = expenses.Sum(e => e.Amount),
            CategoryBreakdown = categoryBreakdown,
            Expenses = expenses.Select(e => new Application.DTOs.Expense.ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Category = e.Category,
                ExpenseDate = e.ExpenseDate,
                Notes = e.Notes
            }).ToList()
        };
    }

    public async Task<ProfitReportDto> GetProfitReportAsync(DateTime startDate, DateTime endDate)
    {
        var endOfEndDate = endDate.Date.AddDays(1);

        var invoices = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue &&
                        i.PaidAt.Value >= startDate && i.PaidAt.Value < endOfEndDate)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate && e.ExpenseDate < endOfEndDate)
            .ToListAsync();

        var totalRevenue = invoices.Sum(i => i.TotalAmount);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var netProfit = totalRevenue - totalExpenses;
        var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;

        var days = (endDate - startDate).Days + 1;
        var dailyBreakdown = Enumerable.Range(0, days).Select(day => new DailyProfitDto
        {
            Date = startDate.AddDays(day),
            Revenue = invoices.Where(i => i.PaidAt!.Value.Date == startDate.AddDays(day).Date).Sum(i => i.TotalAmount),
            Expenses = expenses.Where(e => e.ExpenseDate.Date == startDate.AddDays(day).Date).Sum(e => e.Amount),
            Profit = invoices.Where(i => i.PaidAt!.Value.Date == startDate.AddDays(day).Date).Sum(i => i.TotalAmount) -
                     expenses.Where(e => e.ExpenseDate.Date == startDate.AddDays(day).Date).Sum(e => e.Amount)
        }).ToList();

        return new ProfitReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetProfit = netProfit,
            ProfitMargin = profitMargin,
            DailyBreakdown = dailyBreakdown
        };
    }
}
