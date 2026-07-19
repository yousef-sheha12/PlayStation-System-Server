using PlayStation.Application.DTOs.Report;

namespace PlayStation.Application.Interfaces;

public interface IReportService
{
    Task<DailyRevenueReportDto> GetDailyRevenueAsync(DateTime date);
    Task<MonthlyRevenueReportDto> GetMonthlyRevenueAsync(int year, int month);
    Task<YearlyRevenueReportDto> GetYearlyRevenueAsync(int year);
    Task<List<MostUsedDeviceDto>> GetMostUsedDevicesAsync(int count = 5);
    Task<List<MostSoldProductDto>> GetMostSoldProductsAsync(int count = 5);
    Task<ExpensesReportDto> GetExpensesReportAsync(DateTime startDate, DateTime endDate);
    Task<ProfitReportDto> GetProfitReportAsync(DateTime startDate, DateTime endDate);
}
