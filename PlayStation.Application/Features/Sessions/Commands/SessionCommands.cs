using MediatR;
using PlayStation.Application.DTOs.Session;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Sessions.Commands;

public record StartSessionCommand(StartSessionDto Request) : IRequest<Result<SessionDto>>;
public record PauseSessionCommand(int SessionId) : IRequest<Result>;
public record ResumeSessionCommand(int SessionId) : IRequest<Result>;
public record EndSessionCommand(int SessionId, decimal Discount = 0) : IRequest<Result<SessionDto>>;
public record AddProductToSessionCommand(int SessionId, AddProductToSessionDto Request) : IRequest<Result>;
public record RemoveProductFromSessionCommand(int SessionId, int ProductId) : IRequest<Result>;
