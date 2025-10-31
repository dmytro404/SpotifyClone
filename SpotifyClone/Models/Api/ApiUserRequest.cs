using Microsoft.AspNetCore.Mvc;

namespace SpotifyClone.Models.Api
{
    public class UserLoginRequest
    {
        [FromBody]
        public string Login { get; set; } = null!;

        [FromBody]
        public string Password { get; set; } = null!;
    }

    public class UserSignUpRequest
    {
        [FromBody]
        public string Name { get; set; } = null!;

        [FromBody]
        public string Email { get; set; } = null!;

        [FromBody]
        public string Login { get; set; } = null!;

        [FromBody]
        public string Password { get; set; } = null!;
    }
}