using JwtRefreshToken.Data;
using JwtRefreshToken.Data.Entities;
using JwtRefreshToken.Models;
using JwtRefreshToken.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
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
        public async Task<AuthResponse> GetTokenAsync(AuthRequest authRequest, string ipAddress)
        {
            //get the user 
            var user = _db.Users.FirstOrDefault(it => it.UserName.Equals(authRequest.UserName));

            if (user is null)
            {
                return await Task.FromResult<AuthResponse>(null);
            }

            //compare password 
            var verifyPassword = _passwordService.VerifyPasswordHash(authRequest.Password, user.PasswordHash, user.PasswordSalt);
            if (!verifyPassword)
                return await Task.FromResult<AuthResponse>(null);


            var stringToken = GenerateToken(user.UserName);
            var refreshToken = GenerateRefreshToken();

        

            return await SaveTokenDetails(ipAddress,user.UserId, stringToken, refreshToken);    
        }

        public async Task<AuthResponse> GetRefreshTokenAsync(string ipAddress, int userId, string userName)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateToken(userName);
            return await SaveTokenDetails(ipAddress, userId, accessToken, refreshToken);
        }

        public async Task<bool> IsTokenValid(string accessToken, string ipAddress)
        {
            var isValid = _db.UserRefreshTokens.FirstOrDefault(it => it.Token == accessToken && it.IpAddress == ipAddress) is not null;
            return await Task.FromResult(isValid);
        }

        private async Task<AuthResponse> SaveTokenDetails(string ipAddress,int userId,string tokenString,string refreshToken)
        {
            var userRefreshToken = new UserRefreshToken
            {
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMinutes(3),
                IpAddress = ipAddress,
                IsInvalidated = false,
                RefreshToken = refreshToken,
                Token = tokenString,
                UserId = userId
            };

            await _db.UserRefreshTokens.AddAsync(userRefreshToken);
            await _db.SaveChangesAsync();


            var response = new AuthResponse
            {
                Token = tokenString,
                RefreshToken = refreshToken,
            };

            return await Task.FromResult(response);
        }

        private string GenerateToken(string userName)
        {
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
                    new Claim(ClaimTypes.NameIdentifier, userName)
                }),
                Expires = DateTime.UtcNow.AddSeconds(90),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)

            };

            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {

                randomNumberGenerator.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            }


        }

      
    }
}
