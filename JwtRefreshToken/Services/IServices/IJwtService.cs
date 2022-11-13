using JwtRefreshToken.Models;

namespace JwtRefreshToken.Services.IServices
{
    public interface IJwtService
    {
        Task<string> GetTokenAsync(AuthRequest authRequest);
    }
}
