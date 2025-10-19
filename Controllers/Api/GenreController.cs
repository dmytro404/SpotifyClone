using SpotifyClone.Data;
using SpotifyClone.Models.Home;
using SpotifyClone.Models.Rest;
using SpotifyClone.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/genre")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly DataAccessor _dataAccessor;

        public GenreController(IStorageService storageService, DataAccessor dataAccessor)
        {
            _storageService = storageService;
            _dataAccessor = dataAccessor;
        }

        [HttpPost("addgenre")]
        public RestResponce AddGenre([FromForm]AdminGroupFormModel model)
        {
            var errors = ModelState.Values
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage)
                           .ToArray();

            if (!ModelState.IsValid)
            {
                return new RestResponce
                {
                    Status = RestStatus.Status400,
                    Meta = new()
                    {
                        Service = "Sppotify. Add new genre.",
                        ServerTime = DateTime.Now.Ticks,
                        Errors = errors
                    },
                    Data = null
                };
            }

            var imageExt = _dataAccessor.GetGenreByName(model.Name);
            if(imageExt != null)
            {
                return new RestResponce
                {
                    Status = RestStatus.Status409,
                    Meta = new()
                    {
                        Service = "Sppotify. Add new genre.",
                        ServerTime = DateTime.Now.Ticks,
                        Errors = new string[] { "Genre with this name already exists." }
                    },
                    Data = null
                };

            }

            var genre = new Data.Entities.Genre
            {
                Name = model.Name,
                Description = model.Description,
                Slug = model.Slug,
                ImageUrl = _storageService.Save(model.Image)
            };

            var savedGenre = _dataAccessor.AddGenre(genre);

            return new RestResponce
            {
                Status = RestStatus.Status200,
                Meta = new()
                {
                    Service = "Sppotify. Add new genre.",
                    ServerTime = DateTime.Now.Ticks,  
                },
                Data = savedGenre
            };
        }
        [HttpGet("all")]
        public RestResponce GetAllGenres()
        {
            var genres = _dataAccessor.GetGenres();

            return new RestResponce
            {
                Status = RestStatus.Status200,
                Meta = new()
                {
                    Service = "Spotify. Get all genres.",
                    ServerTime = DateTime.Now.Ticks
                },
                Data = genres
            };
        }
    }
}
