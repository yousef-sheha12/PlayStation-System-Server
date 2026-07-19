using MediatR;
using PlayStation.Application.DTOs.Dashboard;
using PlayStation.Application.Features.Dashboard.Queries;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Dashboard.Handlers;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IDashboardService _dashboardService;

    public GetDashboardHandler(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardAsync();
            return Result<DashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            return Result<DashboardDto>.Failure(ex.Message);
        }
    }
}
