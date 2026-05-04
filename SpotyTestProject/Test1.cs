using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Middleware.Auth;
using SpotifyClone.Services.Kdf;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

        _context.UserRoles.AddRange(
            new UserRole
            {
                Id = "Admin",
                Description = "Administrator",
                CanCreate = true,
                CanRead = true,
                CanUpdate = true,
                CanDelete = true
            },
            new UserRole
            {
                Id = "Guest",
                Description = "Guest",
                CanCreate = false,
                CanRead = false,
                CanUpdate = false,
                CanDelete = false
            }
        );
        _context.SaveChanges();

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
    public void TestPbKdf2Service_SameSaltSameResult()
    {
        var dk1 = _kdfService.Dk("password", "salt");
        var dk2 = _kdfService.Dk("password", "salt");
        Assert.AreEqual(dk1, dk2);
    }

    [TestMethod]
    public void TestPbKdf2Service_DifferentSaltDifferentResult()
    {
        var dk1 = _kdfService.Dk("password", "salt1");
        var dk2 = _kdfService.Dk("password", "salt2");
        Assert.AreNotEqual(dk1, dk2);
    }

    [TestMethod]
    public void TestAuthenticate_ValidUser_ReturnsUserAccess()
    {
        var userAccess = _accessor.Authenticate("Admin", "Admin");
        Assert.IsNotNull(userAccess);
        Assert.AreEqual("Admin", userAccess.RoleId);
    }

    [TestMethod]
    public void TestAuthenticate_WrongPassword_ReturnsNull()
    {
        var userAccess = _accessor.Authenticate("Admin", "WrongPassword");
        Assert.IsNull(userAccess);
    }

    [TestMethod]
    public void TestAuthenticate_UnknownLogin_ReturnsNull()
    {
        var userAccess = _accessor.Authenticate("nobody", "Admin");
        Assert.IsNull(userAccess);
    }

    [TestMethod]
    public void TestAuthenticate_LoadsRole()
    {
        var userAccess = _accessor.Authenticate("Admin", "Admin");
        Assert.IsNotNull(userAccess?.Role);
        Assert.IsTrue(userAccess!.Role.CanRead);
        Assert.IsTrue(userAccess.Role.CanCreate);
    }

    [TestMethod]
    public void TestAuthenticate_LoadsUser()
    {
        var userAccess = _accessor.Authenticate("Admin", "Admin");
        Assert.IsNotNull(userAccess?.User);
        Assert.AreEqual("Default Administrator", userAccess!.User.Name);
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
    public void TestGetAlbums_ReturnsAll()
    {
        _accessor.AddAlbum("A1", "Artist1", "", DateTime.Now);
        _accessor.AddAlbum("A2", "Artist2", "", DateTime.Now);

        var albums = _accessor.GetAlbums().ToList();
        Assert.IsTrue(albums.Count >= 2);
    }

    [TestMethod]
    public void TestAddTrack_GetTrack()
    {
        var album = _accessor.AddAlbum("Album2", "Artist2", "cover2.jpg", DateTime.Now);
        var genre = _context.Genres.First();

        var track = _accessor.AddTrack(
            "Track1", "Artist1", "url1.mp3",
            TimeSpan.FromMinutes(3), DateTime.Now,
            album.Id, genre.Id
        );

        Assert.IsNotNull(track);

        var fetched = _accessor.GetTrack(track.Id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("Track1", fetched.Title);
        Assert.AreEqual(album.Id, fetched.AlbumId);
        Assert.AreEqual(genre.Id, fetched.GenreId);
    }

    private static (string token, IConfiguration config) MakeJwt(
        string secret = "ThisIsASecretKey1234567890123456",
        string role = "Admin",
        string name = "Default Administrator",
        string userId = "1")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "ASP-32",
            audience: "ASP-32",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Jwt:Secret"] = secret
            })
            .Build();

        return (tokenStr, config);
    }

    [TestMethod]
    public void TestJwtAuthMiddleware_ValidToken_SetsUser()
    {
        var (token, config) = MakeJwt();

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        var middleware = new JwtAuthMiddleware(next: _ => Task.CompletedTask);
        middleware.InvokeAsync(context, config).Wait();

        Assert.IsTrue(context.User.Identity?.IsAuthenticated ?? false);
        Assert.AreEqual("Admin", context.User.FindFirst(ClaimTypes.Role)?.Value);
        Assert.AreEqual("Default Administrator", context.User.FindFirst(ClaimTypes.Name)?.Value
                                              ?? context.User.FindFirst(JwtRegisteredClaimNames.Name)?.Value);
    }

    [TestMethod]
    public void TestJwtAuthMiddleware_NoToken_UserNotAuthenticated()
    {
        var (_, config) = MakeJwt();

        var context = new DefaultHttpContext();

        var middleware = new JwtAuthMiddleware(next: _ => Task.CompletedTask);
        middleware.InvokeAsync(context, config).Wait();

        Assert.IsFalse(context.User.Identity?.IsAuthenticated ?? false);
    }

    [TestMethod]
    public void TestJwtAuthMiddleware_InvalidToken_UserNotAuthenticated()
    {
        var (_, config) = MakeJwt();

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer this.is.invalid";

        var middleware = new JwtAuthMiddleware(next: _ => Task.CompletedTask);
        middleware.InvokeAsync(context, config).Wait();

        Assert.IsFalse(context.User.Identity?.IsAuthenticated ?? false);
    }

    [TestMethod]
    public void TestJwtAuthMiddleware_ExpiredToken_UserNotAuthenticated()
    {
        var secret = "ThisIsASecretKey12345678901234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var token = new JwtSecurityToken(
            issuer: "ASP-32",
            audience: "ASP-32",
            claims: new[] { new Claim(ClaimTypes.Role, "Admin") },
            expires: DateTime.UtcNow.AddHours(-1),  // уже истёк
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
            {
                ["Jwt:Secret"] = secret
            })
            .Build();

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {tokenStr}";

        var middleware = new JwtAuthMiddleware(next: _ => Task.CompletedTask);
        middleware.InvokeAsync(context, config).Wait();

        Assert.IsFalse(context.User.Identity?.IsAuthenticated ?? false);
    }
}