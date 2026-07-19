using PlayStation.Application.DTOs.Dashboard;

namespace PlayStation.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
