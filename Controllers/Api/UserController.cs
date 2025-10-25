using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Kdf;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;
        private readonly IAuthService _authService;

        public UserController(DataAccessor dataAccessor, IKdfService kdfService, IAuthService authService)
        {
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            var ua = _dataAccessor.Authenticate(model.Login, model.Password);
            if (ua == null)
                return Unauthorized(new { Status = "Invalid credentials" });

            _authService.SetAuth(ua);

            return Ok(new
            {
                UserId = ua.UserId,
                Name = ua.User.Name,
                Email = ua.User.Email,
                Role = ua.Role.Description
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _authService.RemoveAuth();
            return Ok(new { Status = "Logged out" });
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] SignUpRequest model)
        {
            return Ok(new { Status = "SignUp Works" });
        }

        [HttpPost("admin")]
        public IActionResult SignUpAdmin([FromBody] SignUpRequest model)
        {
            return Ok(new { Status = "SignUpAdmin Works" });
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class SignUpRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}