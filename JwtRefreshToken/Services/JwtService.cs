using JwtRefreshToken.Data;
using JwtRefreshToken.Models;
using JwtRefreshToken.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace JwtRefreshToken.Services
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordService _passwordService;
        public JwtService(ApplicationDbContext db, IConfiguration config, IPasswordService passwordService)
        {
            _db = db;
            _config = config;   
            _passwordService = passwordService;
        }
        public async Task<string> GetTokenAsync(AuthRequest authRequest)
        {
            //get the user 
            var user = _db.Users.FirstOrDefault(it => it.UserName.Equals(authRequest.UserName));

            if (user is null)
            {
                return await Task.FromResult<string>(null);
            }      
             
            

            //compare password 
            var verifyPassword = _passwordService.VerifyPasswordHash(authRequest.Password, user.PasswordHash, user.PasswordSalt);
            if(!verifyPassword)
                return await Task.FromResult<string>(null);

            //get jwt and application keys
            var jwtKey = _config.GetValue<string>("JwtSettings:Key");
            var applicationId = _config.GetValue<string>("Application:ApplicationId");
            var masterKey = $"{jwtKey}:{applicationId}";
            var keyBytes = Encoding.UTF8.GetBytes(masterKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserName)
                }),
                Expires = DateTime.UtcNow.AddSeconds(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)

            };

            var token = tokenHandler.CreateToken(descriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

       
    }
}
