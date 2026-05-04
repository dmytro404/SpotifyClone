using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using System.Security.Claims;

namespace SpotifyClone.Controllers
{
    public class AccountController(DataAccessor dataAccessor) : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login([FromForm] string login, [FromForm] string password)
        {
            var userAccess = dataAccessor.Authenticate(login, password);
            if (userAccess == null)
            {
                ModelState.AddModelError("", "Invalid login or password");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userAccess.User.Name),
                new Claim(ClaimTypes.Email, userAccess.User.Email),
                new Claim(ClaimTypes.Role, userAccess.RoleId)
            };

            var identity = new ClaimsIdentity(claims, "Cookie");
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync("Cookie", principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("Cookie");
            return RedirectToAction("Index", "Home");
        }
    }
}