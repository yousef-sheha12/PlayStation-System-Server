using PlayStation.Application.DTOs.Auth;

namespace PlayStation.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(string token);
}
