using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Middleware.Auth;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Kdf;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[TestClass]
public class Test1
{
    private DataContext _context = null!;
    private DataAccessor _accessor = null!;
    private IKdfService _kdfService = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new DataContext(options);
        _kdfService = new PbKdf2Service();
        _accessor = new DataAccessor(_context, _kdfService);

        _accessor.CreateUser("Default Administrator", "admin@test.dev", "Admin", "Admin", "Admin");

        if (!_context.Genres.Any())
        {
            _context.Genres.Add(new Genre { Id = 1, Name = "Pop" });
            _context.SaveChanges();
        }
    }

    [TestMethod]
    public void TestPbKdf2Service_Dk()
    {
        var dk = _kdfService.Dk("password", "salt");
        Assert.IsNotNull(dk);
        Assert.AreEqual(64, dk.Length);
    }

    [TestMethod]
    public void TestAuthenticate_ValidUser_ReturnsUserAccess()
    {
        var userAccess = _accessor.Authenticate("Admin", "Admin");
        Assert.IsNotNull(userAccess);
        Assert.AreEqual("Admin", userAccess.RoleId);
    }

    [TestMethod]
    public void TestCreateUser_AddsUser()
    {
        var user = _accessor.CreateUser("Test User", "test@test.dev", "testuser", "123", "Guest");
        Assert.IsNotNull(user);

        var auth = _accessor.Authenticate("testuser", "123");
        Assert.IsNotNull(auth);
        Assert.AreEqual("testuser", auth.Login);
        Assert.AreEqual("Guest", auth.RoleId);
    }

    [TestMethod]
    public void TestAddAlbum_GetAlbum()
    {
        var album = _accessor.AddAlbum("Album1", "Artist1", "cover1.jpg", DateTime.Now);
        Assert.IsNotNull(album);

        var fetched = _accessor.GetAlbum(album.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual(album.Title, fetched.Title);
    }

    [TestMethod]
    public void TestAddTrack_GetTrack()
    {
        var album = _accessor.AddAlbum("Album2", "Artist2", "cover2.jpg", DateTime.Now);
        var genre = _context.Genres.First();

        var track = _accessor.AddTrack(
            "Track1",
            "Artist1",
            "url1.mp3",
            TimeSpan.FromMinutes(3),
            DateTime.Now,
            album.Id,
            genre.Id
        );

        Assert.IsNotNull(track);

        var fetched = _accessor.GetTrack(track.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("Track1", fetched.Title);
        Assert.AreEqual(album.Id, fetched.AlbumId);
        Assert.AreEqual(genre.Id, fetched.GenreId);
    }

    [TestMethod]
    public void TestJwtAuthMiddleware_ValidToken_SetsUser()
    {
        var secret = "ThisIsASecretKey123";
        var payload = new { sub = "1", name = "Admin", aud = "Admin" };
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));

        var headerBase64 = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes("{}"));
        var tokenBody = $"{headerBase64}.{payloadBase64}";
        var signature = Base64UrlTextEncoder.Encode(
            System.Security.Cryptography.HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(secret),
                Encoding.UTF8.GetBytes(tokenBody)
            )
        );
        var jwt = $"{tokenBody}.{signature}";

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {jwt}";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Jwt:Secret"] = secret
            })
            .Build();

        var middleware = new JwtAuthMiddleware(next: (ctx) => Task.CompletedTask);
        middleware.InvokeAsync(context, configuration).Wait();

        Assert.IsTrue(context.User.Identity?.IsAuthenticated ?? false);
        Assert.AreEqual("Admin", context.User.FindFirst(ClaimTypes.Role)?.Value);
        Assert.AreEqual("Admin", context.User.FindFirst(ClaimTypes.Name)?.Value);
    }

    [TestMethod]
    public void TestSessionAuthMiddleware_SetsUser()
    {
        var userAccess = _context.UserAccesses
            .Include(ua => ua.User)
            .FirstOrDefault(ua => ua.Login == "Admin")!;

        var context = new DefaultHttpContext();
        var middleware = new SessionAuthMiddleware(next: (ctx) => Task.CompletedTask);

        var authService = new TestAuthService(userAccess);
        middleware.InvokeAsync(context, authService).Wait();

        Assert.IsTrue(context.User.Identity?.IsAuthenticated ?? false);
        Assert.AreEqual("Admin", context.User.FindFirst(ClaimTypes.Role)?.Value);
        Assert.AreEqual("Default Administrator", context.User.FindFirst(ClaimTypes.Name)?.Value);
    }

    public class TestAuthService : IAuthService
    {
        private object? _payload;

        public TestAuthService() { }
        public TestAuthService(object payload) => _payload = payload;

        public void SetAuth(object payload) => _payload = payload;

        public T? GetAuth<T>() where T : notnull
        {
            return (T?)_payload;
        }
        public void RemoveAuth() => _payload = null;
    }
}