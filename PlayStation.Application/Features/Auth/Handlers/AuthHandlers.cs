using MediatR;
using PlayStation.Application.DTOs.Auth;
using PlayStation.Application.Features.Auth.Commands;
using PlayStation.Application.Interfaces;
using PlayStation.Domain.Common;

namespace PlayStation.Application.Features.Auth.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public LoginHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(request.LoginDto);
            return Result<AuthResponseDto>.Success(result, "Login successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result<AuthResponseDto>.Failure(ex.Message);
        }
    }
}
