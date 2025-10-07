using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models;
using SpotifyClone.Models.Home;
using SpotifyClone.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SpotifyClone.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IKdfService _kdfService;
        private readonly DataContext _dataContext;
        private readonly DataAccessor _dataAccessor;

        public HomeController(ILogger<HomeController> logger, IKdfService kdfService, DataContext dataContext, DataAccessor dataAccessor)
        {
            _logger = logger;
            _kdfService = kdfService;
            _dataContext = dataContext;
            _dataAccessor = dataAccessor;
        }

        public IActionResult Index()
        {

            HomeIndexViewModel model = new()
            {
                Genres = _dataAccessor.GetGenres()
            };
            return View(model);
        }

        public IActionResult Admin()
        {
            //bool isAdmin = true;
            bool isAdmin = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)
                ?.Value == "Admin";

            if (!isAdmin)
                return RedirectToAction(nameof(Index));

            HomeAdminViewModel model = new()
            {
                Genres = _dataContext.Genres
                    .Where(g => g.DeletedAt == null)
                    .AsNoTracking()
                    .ToList()
            };

            return View(new AdminGroupFormModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
