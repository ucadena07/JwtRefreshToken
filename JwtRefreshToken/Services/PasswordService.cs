using JwtRefreshToken.Models.Dtos;
using JwtRefreshToken.Services.IServices;
using System.Security.Cryptography;
using System.Text;

namespace JwtRefreshToken.Services
{
    public class PasswordService : IPasswordService
    {
        public PasswordHelperDto CreatePasswordHash(string password)
        {
            PasswordHelperDto obj = new();
            using (var hmac = new HMACSHA512())
            {
                obj.passwordSalt = hmac.Key;
                obj.passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            return obj;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
