using MediatR;
using PlayStation.Application.DTOs.Auth;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Auth.Commands;

public record LoginCommand(LoginDto LoginDto) : IRequest<Result<AuthResponseDto>>;
