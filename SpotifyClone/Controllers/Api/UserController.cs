using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController(
        DataAccessor dataAccessor,
        IConfiguration configuration
    ) : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Login) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { status = "Login and password are required" });

            var login = model.Login.Trim();
            var userAccess = dataAccessor.Authenticate(login, model.Password);
            if (userAccess == null)
                return Unauthorized(new { status = "Invalid credentials" });

            var token = GenerateJwtToken(userAccess);

            return Ok(new
            {
                status = "Success",
                token,
                userId = userAccess.UserId,
                userName = userAccess.User.Name,
                email = userAccess.User.Email,
                login,
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
            if (
                model == null ||
                string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Login) ||
                string.IsNullOrWhiteSpace(model.Password)
            )
            {
                return BadRequest(new { status = "Name, email, login, and password are required" });
            }

            var login = model.Login.Trim();
            if (dataAccessor.LoginExists(login))
                return BadRequest(new { status = "Login already exists" });

            User user;
            UserAccess? userAccess;

            try
            {
                user = dataAccessor.CreateUser(model.Name, model.Email, login, model.Password, "Guest");
                userAccess = dataAccessor.Authenticate(login, model.Password);
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { status = "Login already exists" });
            }

            if (userAccess == null)
                return StatusCode(500, new { status = "User was created but authentication failed" });

            var token = GenerateJwtToken(userAccess);

            return Ok(new
            {
                status = "Success",
                token,
                userId = user.Id,
                userName = user.Name,
                name = user.Name,
                email = user.Email,
                login,
                role = userAccess.RoleId
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
