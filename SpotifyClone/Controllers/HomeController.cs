using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Home;
using System.Security.Claims;

public class HomeController : Controller
{
    private readonly DataContext _context;

    public HomeController(DataContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var albums = _context.Albums.ToList();

        var model = new HomeAdminViewModel
        {
            Albums = albums
        };

        return View(model);
    }

    public IActionResult Admin()
    {
        var albums = _context.Albums
            .Include(a => a.Tracks)
            .ToList();

        var tracks = _context.Tracks
            .Include(t => t.Album)
            .Select(t => new TrackAdminViewModel
            {
                Id = t.Id,
                Title = t.Title,
                AlbumId = t.AlbumId,
                AlbumTitle = t.Album.Title,
                Duration = t.Duration
            })
            .ToList();

        var model = new HomeAdminViewModel
        {
            Albums = albums,
            Tracks = tracks
        };

        return View(model);
    }
}