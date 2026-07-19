using MediatR;
using PlayStation.Application.DTOs.Dashboard;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Dashboard.Queries;

public record GetDashboardQuery() : IRequest<Result<DashboardDto>>;
