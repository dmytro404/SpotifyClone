using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Auth;
using System.Security.Claims;

namespace SpotifyClone.Controllers
{
    public class AccountController(
        DataAccessor dataAccessor,
        IAuthService authService) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IAuthService _authService = authService;

        // GET: /Account/Login (можно для теста)
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login([FromForm] string login, [FromForm] string password)
        {
            try
            {
                UserAccess? userAccess = _dataAccessor.Authenticate(login, password);
                if (userAccess == null)
                {
                    ModelState.AddModelError("", "Invalid login or password");
                    return RedirectToAction("Index", "Home");
                }

                // Создаем Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userAccess.User.Name),
                    new Claim(ClaimTypes.Email, userAccess.User.Email),
                    new Claim(ClaimTypes.Role, userAccess.Role?.Description ?? "User")
                };

                var identity = new ClaimsIdentity(claims, "Cookie");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync("Cookie", principal);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("Cookie");
            return RedirectToAction("Index", "Home");
        }
    }
}