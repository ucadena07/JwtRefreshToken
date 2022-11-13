using JwtRefreshToken.Data;
using JwtRefreshToken.Data.Entities;
using JwtRefreshToken.Models;
using JwtRefreshToken.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace JwtRefreshToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private APIResponse _response;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _paswordService;
        private readonly ApplicationDbContext _db;
        public AccountController(IJwtService jwtService, IPasswordService passwordService,ApplicationDbContext db)
        {
            _response = new();
            _jwtService = jwtService;
            _paswordService = passwordService;
            _db = db;   
        }

        [HttpPost("AuthToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> AuthToken([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username and password must be provided");
                return BadRequest(_response);
            }

            var authoResponse = await _jwtService.GetTokenAsync(request, HttpContext.Connection.RemoteIpAddress.ToString());
            if(authoResponse is null)
            {
                
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                return Unauthorized(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = authoResponse;
            return Ok(_response);

        }

        [HttpPost("RefreshToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Tokens must be provided");
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            var token = GetJwtToken(request.ExpiredToken);

            var userRefreshToken = _db.UserRefreshTokens.FirstOrDefault(it => it.IsInvalidated == false
            && it.Token == request.ExpiredToken &&
            it.RefreshToken == request.RefreshToken
            && it.IpAddress == ipAddress);

            AuthResponse authReponse = ValidateDetails(token, userRefreshToken);
            if (!authReponse.IsSuccess)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add(authReponse.Reason.ToString());
                return BadRequest(_response);
            }

            userRefreshToken.IsInvalidated = true;
            _db.UserRefreshTokens.Update(userRefreshToken);
            await _db.SaveChangesAsync();


            var userName = token.Claims.FirstOrDefault(it => it.Type == JwtRegisteredClaimNames.NameId).Value;
            var dbReponse = await _jwtService.GetRefreshTokenAsync(ipAddress, userRefreshToken.UserId,userName);

            _response.Result = dbReponse;


            return _response;
        }

        private AuthResponse ValidateDetails(JwtSecurityToken token, UserRefreshToken userRefreshToken)
        {
            if(userRefreshToken is null)
                return new AuthResponse { IsSuccess= false, Reason = "Invalid Token Details" };
            if(token.ValidTo > DateTime.UtcNow)
                return new AuthResponse { IsSuccess = false, Reason = "Token not expired" };
            if(!userRefreshToken.IsActive)
                return new AuthResponse { IsSuccess = false, Reason = "Refresh token expired" };
            return new AuthResponse { IsSuccess = true };

        }

        private JwtSecurityToken GetJwtToken(string expiredToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(expiredToken);
        }

        //[HttpPost("Register")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<APIResponse>> Register([FromBody] AuthRequest request)
        //{
        //    User user = new();
        //    user.UserName = "admin@test.com";

        //    var passcodes = _paswordService.CreatePasswordHash("Admin123*");

        //    user.PasswordHash = passcodes.passwordHash;
        //    user.PasswordSalt = passcodes.passwordSalt;

        //    _db.Add(user);
        //    await _db.SaveChangesAsync();
                
        //    return Ok(_response);   
        //}

    }
}
