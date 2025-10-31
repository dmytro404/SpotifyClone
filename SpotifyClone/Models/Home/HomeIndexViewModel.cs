using SpotifyClone.Data.Entities;
using System.Collections.Generic;

namespace SpotifyClone.Models.Home
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();
    }
}