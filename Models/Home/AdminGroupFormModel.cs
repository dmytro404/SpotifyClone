using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SpotifyClone.Models.Home
{
    public class AdminGroupFormModel
    {
        [Required(ErrorMessage = "Поле є обов'язкове.")]
        [FromForm(Name = "genre-name")]
        public String Name { get; set; } = null!;

        [Required(ErrorMessage = "Поле є обов'язкове.")]
        [FromForm(Name = "genre-description")]
        public String Description { get; set; } = null!;

        [Required(ErrorMessage = "Поле є обов'язкове.")]
        [FromForm(Name = "genre-slug")]
        public String Slug { get; set; } = null!;

        [Required(ErrorMessage = "Поле є обов'язкове.")]
        [FromForm(Name = "genre-image")]
        public IFormFile Image { get; set; } = null!;
    }
}
