using MediatR;
using PlayStation.Application.DTOs.Report;
using PlayStation.Application.Features.Reports.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Reports.Handlers;

public class GetDailyRevenueHandler : IRequestHandler<GetDailyRevenueQuery, Result<DailyRevenueReportDto>>
{
    private readonly IReportService _reportService;

    public GetDailyRevenueHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<DailyRevenueReportDto>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetDailyRevenueAsync(request.Date);
            return Result<DailyRevenueReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<DailyRevenueReportDto>.Failure(ex.Message);
        }
    }
}

public class GetMonthlyRevenueHandler : IRequestHandler<GetMonthlyRevenueQuery, Result<MonthlyRevenueReportDto>>
{
    private readonly IReportService _reportService;

    public GetMonthlyRevenueHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<MonthlyRevenueReportDto>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetMonthlyRevenueAsync(request.Year, request.Month);
            return Result<MonthlyRevenueReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<MonthlyRevenueReportDto>.Failure(ex.Message);
        }
    }
}

public class GetYearlyRevenueHandler : IRequestHandler<GetYearlyRevenueQuery, Result<YearlyRevenueReportDto>>
{
    private readonly IReportService _reportService;

    public GetYearlyRevenueHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<YearlyRevenueReportDto>> Handle(GetYearlyRevenueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetYearlyRevenueAsync(request.Year);
            return Result<YearlyRevenueReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<YearlyRevenueReportDto>.Failure(ex.Message);
        }
    }
}

public class GetMostUsedDevicesHandler : IRequestHandler<GetMostUsedDevicesQuery, Result<List<MostUsedDeviceDto>>>
{
    private readonly IReportService _reportService;

    public GetMostUsedDevicesHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<List<MostUsedDeviceDto>>> Handle(GetMostUsedDevicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetMostUsedDevicesAsync(request.Count);
            return Result<List<MostUsedDeviceDto>>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<List<MostUsedDeviceDto>>.Failure(ex.Message);
        }
    }
}

public class GetMostSoldProductsHandler : IRequestHandler<GetMostSoldProductsQuery, Result<List<MostSoldProductDto>>>
{
    private readonly IReportService _reportService;

    public GetMostSoldProductsHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<List<MostSoldProductDto>>> Handle(GetMostSoldProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetMostSoldProductsAsync(request.Count);
            return Result<List<MostSoldProductDto>>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<List<MostSoldProductDto>>.Failure(ex.Message);
        }
    }
}

public class GetExpensesReportHandler : IRequestHandler<GetExpensesReportQuery, Result<ExpensesReportDto>>
{
    private readonly IReportService _reportService;

    public GetExpensesReportHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<ExpensesReportDto>> Handle(GetExpensesReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetExpensesReportAsync(request.StartDate, request.EndDate);
            return Result<ExpensesReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<ExpensesReportDto>.Failure(ex.Message);
        }
    }
}

public class GetProfitReportHandler : IRequestHandler<GetProfitReportQuery, Result<ProfitReportDto>>
{
    private readonly IReportService _reportService;

    public GetProfitReportHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<ProfitReportDto>> Handle(GetProfitReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetProfitReportAsync(request.StartDate, request.EndDate);
            return Result<ProfitReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<ProfitReportDto>.Failure(ex.Message);
        }
    }
}
