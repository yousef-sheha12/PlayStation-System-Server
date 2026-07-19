using MediatR;
using PlayStation.Application.DTOs.Session;
using PlayStation.Domain.Common;
using PlayStation.Domain.Enums;

namespace PlayStation.Application.Features.Sessions.Queries;

public record GetAllSessionsQuery() : IRequest<Result<List<SessionDto>>>;
public record GetSessionByIdQuery(int Id) : IRequest<Result<SessionDto>>;
public record GetActiveSessionsQuery() : IRequest<Result<List<SessionDto>>>;
public record GetSessionsByStatusQuery(SessionStatus Status) : IRequest<Result<List<SessionDto>>>;
public record GetSessionsByDeviceQuery(int DeviceId) : IRequest<Result<List<SessionDto>>>;
public record GetSessionsByDateRangeQuery(DateTime StartDate, DateTime EndDate) : IRequest<Result<List<SessionDto>>>;
