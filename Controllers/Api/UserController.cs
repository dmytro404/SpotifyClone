using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Api;
using SpotifyClone.Services.Kdf;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IConfiguration _configuration;

        public UserController(DataAccessor dataAccessor, IKdfService kdfService, IConfiguration configuration)
        {
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest model)
        {
            var userAccess = _dataAccessor.Authenticate(model.Login, model.Password);
            if (userAccess == null)
                return Unauthorized(new { Status = "Invalid credentials" });

            HttpContext.Session.SetString("UserName", userAccess.User.Name);
            HttpContext.Session.SetString("UserRole", userAccess.RoleId);

            return Ok(new
            {
                Status = "Success",
                userName = userAccess.User.Name
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserName");
            return Ok(new { Status = "Logged out" });
        }


        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] UserSignUpRequest model)
        {
            var existingByLogin = _dataAccessor.Authenticate(model.Login, model.Password);
            if (existingByLogin != null)
                return BadRequest(new { Status = "Login already exists" });

            var user = _dataAccessor.CreateUser(model.Name, model.Email, model.Login, model.Password, "user");
            var userAccess = _dataAccessor.Authenticate(model.Login, model.Password);

            var token = GenerateJwtToken(userAccess!);

            HttpContext.Session.SetString("UserName", userAccess.User.Name);
            HttpContext.Session.SetString("UserRole", userAccess.RoleId);

            return Ok(new
            {
                token,
                userId = user.Id,
                name = user.Name,
                email = user.Email,
                role = userAccess!.RoleId
            });
        }

        private string GenerateJwtToken(UserAccess userAccess)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userAccess.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, userAccess.User.Name),
                new Claim(JwtRegisteredClaimNames.Email, userAccess.User.Email),
                new Claim(ClaimTypes.Role, userAccess.RoleId)
            };

            var token = new JwtSecurityToken(
                issuer: "ASP-32",
                audience: "ASP-32",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserLoginRequest
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserSignUpRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
