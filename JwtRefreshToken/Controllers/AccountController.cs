using JwtRefreshToken.Data;
using JwtRefreshToken.Data.Entities;
using JwtRefreshToken.Models;
using JwtRefreshToken.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            var token = await _jwtService.GetTokenAsync(request);
            if(token is null)
            {
                
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.IsSuccess = false;
                return Unauthorized(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = token;
            return Ok(_response);

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
