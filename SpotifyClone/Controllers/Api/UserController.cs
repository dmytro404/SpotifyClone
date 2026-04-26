using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Kdf;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController(
        DataAccessor dataAccessor,
        IKdfService kdfService,
        IConfiguration configuration
    ) : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest model)
        {
            var userAccess = dataAccessor.Authenticate(model.Login, model.Password);
            if (userAccess == null)
                return Unauthorized(new { status = "Invalid credentials" });

            var token = GenerateJwtToken(userAccess);

            return Ok(new
            {
                status = "Success",
                token,
                userName = userAccess.User.Name,
                role = userAccess.RoleId
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { status = "Logged out" });
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] UserSignUpRequest model)
        {
            var existingByLogin = dataAccessor.Authenticate(model.Login, model.Password);
            if (existingByLogin != null)
                return BadRequest(new { status = "Login already exists" });

            var user = dataAccessor.CreateUser(model.Name, model.Email, model.Login, model.Password, "Guest");
            var userAccess = dataAccessor.Authenticate(model.Login, model.Password);

            var token = GenerateJwtToken(userAccess!);

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
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);
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