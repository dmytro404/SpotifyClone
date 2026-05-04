using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Models.Rest;

namespace SpotifyClone.Controllers.Api
{
    [ApiController]
    public class GenresController(DataContext dataContext) : ControllerBase
    {
        [HttpGet("api/genres")]
        [HttpGet("api/genre/all")]
        public IActionResult GetAll()
        {
            var data = dataContext.Genres
                .OrderBy(genre => genre.Name)
                .Select(genre => new
                {
                    genre.Id,
                    genre.Name
                })
                .ToList();

            return Ok(new { status = RestStatus.Status200, data });
        }
    }
}
