using MediatR;
using PlayStation.Application.DTOs.Report;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Reports.Queries;

public record GetDailyRevenueQuery(DateTime Date) : IRequest<Result<DailyRevenueReportDto>>;
public record GetMonthlyRevenueQuery(int Year, int Month) : IRequest<Result<MonthlyRevenueReportDto>>;
public record GetYearlyRevenueQuery(int Year) : IRequest<Result<YearlyRevenueReportDto>>;
public record GetMostUsedDevicesQuery(int Count) : IRequest<Result<List<MostUsedDeviceDto>>>;
public record GetMostSoldProductsQuery(int Count) : IRequest<Result<List<MostSoldProductDto>>>;
public record GetExpensesReportQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<ExpensesReportDto>>;
public record GetProfitReportQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<ProfitReportDto>>;
