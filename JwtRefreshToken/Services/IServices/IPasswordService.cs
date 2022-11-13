using JwtRefreshToken.Models.Dtos;

namespace JwtRefreshToken.Services.IServices
{
    public interface IPasswordService
    {
        PasswordHelperDto CreatePasswordHash(string password);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
