using PlayStation.Application.DTOs.Dashboard;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Enums;
using PlayStation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PlayStation.Infrastructure.Services;

internal class DeviceSessionInfo
{
    public int DeviceId { get; set; }
    public DateTime LatestStart { get; set; }
}

public class DashboardService : IDashboardService
{
    private readonly PlayStationDbContext _context;

    public DashboardService(PlayStationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var todayRevenue = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue && i.PaidAt.Value.Date == today)
            .SumAsync(i => i.TotalAmount);

        var monthlyRevenue = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue && i.PaidAt.Value >= firstDayOfMonth)
            .SumAsync(i => i.TotalAmount);

        var activeSessions = await _context.Sessions
            .CountAsync(s => !s.IsDeleted && (s.Status == SessionStatus.Active || s.Status == SessionStatus.Paused));

        var totalDevices = await _context.Devices.CountAsync(d => !d.IsDeleted);
        var availableDevices = await _context.Devices.CountAsync(d => !d.IsDeleted && d.Status == DeviceStatus.Available);
        var occupiedDevices = await _context.Devices.CountAsync(d => !d.IsDeleted && d.Status == DeviceStatus.Occupied);

        var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
        var lowStockCount = await _context.Products.CountAsync(p => !p.IsDeleted && p.Quantity <= p.LowStockThreshold && p.Quantity > 0);

        var totalCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted);

        var todayExpenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate.Date == today)
            .SumAsync(e => e.Amount);

        var monthlyExpenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= firstDayOfMonth)
            .SumAsync(e => e.Amount);

        var devices = await _context.Devices.Where(d => !d.IsDeleted).ToListAsync();
        var occupiedDeviceSessions = await _context.Sessions
            .Where(s => !s.IsDeleted && s.Status == SessionStatus.Active)
            .GroupBy(s => s.DeviceId)
            .Select(g => new DeviceSessionInfo { DeviceId = g.Key, LatestStart = g.Max(s => s.StartTime) })
            .ToListAsync();

        var deviceSummaries = devices.Select(d =>
        {
            var activeSession = occupiedDeviceSessions.FirstOrDefault(s => s.DeviceId == d.Id);
            var activeMinutes = activeSession != null
                ? (int)(DateTime.UtcNow - activeSession.LatestStart).TotalMinutes
                : 0;
            return new DeviceStatusSummaryDto
            {
                DeviceName = d.Name,
                Status = d.Status.ToString(),
                HourlyRate = d.HourlyRate,
                ActiveMinutes = activeMinutes
            };
        }).ToList();

        var revenueChart = await BuildRevenueChartData(firstDayOfMonth, today);
        var expenseChart = await BuildExpenseChartData(firstDayOfMonth, today);
        var deviceUsageChart = BuildDeviceUsageChartData(devices, occupiedDeviceSessions);
        var sessionChart = await BuildSessionChartData(firstDayOfMonth, today);

        return new DashboardDto
        {
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue,
            ActiveSessions = activeSessions,
            TotalDevices = totalDevices,
            AvailableDevices = availableDevices,
            OccupiedDevices = occupiedDevices,
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            TotalCustomers = totalCustomers,
            TodayExpenses = todayExpenses,
            MonthlyExpenses = monthlyExpenses,
            DeviceSummaries = deviceSummaries,
            RevenueChart = revenueChart,
            ExpenseChart = expenseChart,
            DeviceUsageChart = deviceUsageChart,
            SessionChart = sessionChart
        };
    }

    private async Task<RevenueChartData> BuildRevenueChartData(DateTime startDate, DateTime today)
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-i))
            .Reverse()
            .ToList();

        var dailyRevenues = await _context.Invoices
            .Where(i => !i.IsDeleted && i.IsPaid && i.PaidAt.HasValue && i.PaidAt.Value.Date >= startDate)
            .GroupBy(i => i.PaidAt!.Value.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(i => i.TotalAmount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        var dailyExpenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate)
            .GroupBy(e => e.ExpenseDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        var dailyRevenue = last7Days.Select(date => new DailyMetric
        {
            Date = date.ToString("MMM dd"),
            Value = dailyRevenues.GetValueOrDefault(date, 0)
        }).ToList();

        var dailyExpense = last7Days.Select(date => new DailyMetric
        {
            Date = date.ToString("MMM dd"),
            Value = dailyExpenses.GetValueOrDefault(date, 0)
        }).ToList();

        var dailyProfit = last7Days.Select(date => new DailyMetric
        {
            Date = date.ToString("MMM dd"),
            Value = dailyRevenues.GetValueOrDefault(date, 0) - dailyExpenses.GetValueOrDefault(date, 0)
        }).ToList();

        return new RevenueChartData
        {
            DailyRevenue = dailyRevenue,
            DailyExpenses = dailyExpense,
            DailyProfit = dailyProfit
        };
    }

    private async Task<ExpenseChartData> BuildExpenseChartData(DateTime startDate, DateTime today)
    {
        var expensesByCategory = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate)
            .GroupBy(e => e.Category)
            .Select(g => new CategoryMetric
            {
                Category = g.Key ?? "Uncategorized",
                Amount = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-i))
            .Reverse()
            .ToList();

        var dailyExpenses = await _context.Expenses
            .Where(e => !e.IsDeleted && e.ExpenseDate >= startDate)
            .GroupBy(e => e.ExpenseDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Date, x => x.Amount);

        var dailyExpense = last7Days.Select(date => new DailyMetric
        {
            Date = date.ToString("MMM dd"),
            Value = dailyExpenses.GetValueOrDefault(date, 0)
        }).ToList();

        return new ExpenseChartData
        {
            ByCategory = expensesByCategory,
            DailyExpenses = dailyExpense
        };
    }

    private DeviceUsageChartData BuildDeviceUsageChartData(
        List<Domain.Entities.Device> devices,
        List<DeviceSessionInfo> occupiedDeviceSessions)
    {
        var usageByDevice = devices.Select(d =>
        {
            var activeSession = occupiedDeviceSessions.FirstOrDefault(s => s.DeviceId == d.Id);
            var usageHours = activeSession != null
                ? (decimal)(DateTime.UtcNow - activeSession.LatestStart).TotalHours
                : 0;
            return new DeviceUsageMetric
            {
                DeviceName = d.Name,
                UsageHours = Math.Round(usageHours, 1),
                Revenue = Math.Round(usageHours * d.HourlyRate, 2)
            };
        }).ToList();

        var statusDistribution = devices
            .GroupBy(d => d.Status.ToString())
            .Select(g => new StatusDistributionMetric
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToList();

        return new DeviceUsageChartData
        {
            UsageByDevice = usageByDevice,
            StatusDistribution = statusDistribution
        };
    }

    private async Task<SessionChartData> BuildSessionChartData(DateTime startDate, DateTime today)
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-i))
            .Reverse()
            .ToList();

        var sessionsByHour = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime.Date >= startDate)
            .GroupBy(s => s.StartTime.Hour)
            .Select(g => new HourlyMetric
            {
                Hour = g.Key,
                SessionCount = g.Count()
            })
            .OrderBy(x => x.Hour)
            .ToListAsync();

        var dailySessions = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime.Date >= startDate)
            .GroupBy(s => s.StartTime.Date)
            .Select(g => new DailyMetric
            {
                Date = g.Key.ToString("MMM dd"),
                Value = g.Count()
            })
            .ToListAsync();

        var topDevices = await _context.Sessions
            .Where(s => !s.IsDeleted && s.StartTime.Date >= startDate)
            .Include(s => s.Device)
            .GroupBy(s => new { s.DeviceId, DeviceName = s.Device.Name })
            .Select(g => new DeviceSessionMetric
            {
                DeviceName = g.Key.DeviceName,
                SessionCount = g.Count(),
                Revenue = g.Sum(s => s.TotalCost)
            })
            .OrderByDescending(x => x.SessionCount)
            .Take(5)
            .ToListAsync();

        return new SessionChartData
        {
            SessionsByHour = sessionsByHour,
            DailySessions = dailySessions,
            TopDevices = topDevices
        };
    }
}
