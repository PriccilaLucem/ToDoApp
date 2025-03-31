using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.View;
using WebApplication.Dto.login;
using WebApplication.Util;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApplication.Config;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("/api/v1/login")]
    public class LoginController(UserViews userViews, ILogger<UserController> logger, JwtSettings jwtSettings) : ControllerBase
    {
        private readonly UserViews _userViews = userViews;
        private readonly ILogger<UserController> _logger = logger;
        private readonly JwtSettings _jwtSettings = jwtSettings;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LoginResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError,  Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
           try
           {

                if (login == null)
                {
                    return BadRequest (new { erro = "Invalid Data" });
                }
                UserModel user = await _userViews.GetUserByEmail(login.Email);
               
                if (!Auth.VerifyPasswordUtil(login.Password, user.Password))
                {
                    return BadRequest (new { erro = "Invald Email or Password"});
                }
               
                var handler =  new JwtSecurityTokenHandler();   
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
                var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.DateOfBirth, user.BirthDate.ToString())

                    ]),
                    Expires = DateTime.UtcNow.AddHours(10),
                    SigningCredentials = credentials
                };

                var token = handler.CreateToken(tokenDescriptor);
                var strToken = handler.WriteToken(token);

                return Ok(new { token = strToken });

           }
           catch(Exception ex)
           {
                return StatusCode(500, new { erro = "Internal Server Error", details = ex.Message });

           }
        }
    }
}