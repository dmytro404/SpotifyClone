using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Data;
using SpotifyClone.Data.Entities;
using SpotifyClone.Models.Rest;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Kdf;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace SpotifyClone.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserController(
            DataContext dataContext,
            IKdfService kdfService,
            IAuthService authService,
            IConfiguration configuration,
            DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IAuthService _authService = authService;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("jwt")]

        public RestResponce AuthenticateJwt()
        {
            RestResponce responce = new();
            UserAccess userAccess;
            try
            {
                var (login, password) = GetBasicCredentials();
                userAccess = _dataAccessor.Authenticate(login, password)
                    ?? throw new Exception("Credintials rejected1");
            }
            catch (Exception ex)
            {
                responce.Status = RestStatus.Status401;
                responce.Data = ex.Message;
                return responce;
            }

            var headerObject = new
            {
                alg = "HS256",
                typ = "JWT"
            };
            String headerJson = JsonSerializer.Serialize(headerObject);
            String header64 = Base64UrlTextEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(headerJson));

            var payloadObject = new

            {

                iss = "ASP-32",   // Issuer	Identifies principal that issued the JWT.

                sub = userAccess.UserId,   // Subject	Identifies the subject of the JWT.

                aud = userAccess.RoleId,   // Audience	Identifies the recipients that the JWT is intended for. Each principal intended to process the JWT must identify itself with a value in the audience claim. If the principal processing the claim does not identify itself with a value in the aud claim when this claim is present, then the JWT must be rejected.

                exp = DateTime.Now.AddMinutes(10),   // Expiration Time	Identifies the expiration time on and after which the JWT must not be accepted for processing. The value must be a NumericDate:[9] either an integer or decimal, representing seconds past 1970-01-01 00:00:00Z.

                nbf = DateTime.Now,   // Not Before	Identifies the time on which the JWT will start to be accepted for processing. The value must be a NumericDate.

                iat = DateTime.Now,   // Issued at	Identifies the time at which the JWT was issued. The value must be a NumericDate.

                jti = Guid.NewGuid(),   // JWT ID	Case-sensitive unique identifier of the token even among different issuers.iss	Issuer	Identifies principal that issued the JWT.

                name = userAccess.User.Name,
                email = userAccess.User.Email,

            };


            String payloadJson = JsonSerializer.Serialize(payloadObject);
            String payload64 = Base64UrlTextEncoder.Encode(System.Text.Encoding.UTF8.GetBytes(payloadJson));

            String secret = _configuration.GetSection("Jwt").GetSection("Secret").Value
                ?? throw new KeyNotFoundException("Not found configuration 'Jwt.Secret'");

            String tokenBody = header64 + '.' + payload64;

            String signature = Base64UrlTextEncoder.Encode(
                System.Security.Cryptography.HMACSHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(secret),
                System.Text.Encoding.UTF8.GetBytes(tokenBody)
                ));

            responce.Data = tokenBody + '.' + signature;

            return responce;
        }

        [HttpGet("me")]
        public RestResponce GetCurrentUser()
        {
            var userId = HttpContext.User.FindFirst("id")?.Value;
            if (userId == null)
                return new RestResponce { Status = RestStatus.Status401 };

            var user = _dataAccessor.GetUserById(Guid.Parse(userId));
            if (user == null)
                return new RestResponce { Status = RestStatus.Status404 };

            return new RestResponce
            {
                Status = RestStatus.Status200,
                Data = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.AvatarUrl
                }
            };
        }


        public RestResponce Authenticate()
        {
            RestResponce restResponce = new()
            {
                Meta = new RestMeta
                {
                    Service = "ASP 32. AuthService",
                    Url = HttpContext.Request.Path,
                    Cache = 0,
                    Manipulations = new[] { "AUTH" },
                    DataType = "UserAccess"
                }
            };

            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                restResponce.Status = RestStatus.Status401;
                return restResponce;
            }
            try
            {
                String credentials =    // 'Basic ' - length = 6
                header[6..];        // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
                String userPass =       // Aladdin:open sesame
                    System.Text.Encoding.UTF8.GetString(
                        Convert.FromBase64String(credentials));

                String[] parts = userPass.Split(':', 2);
                String login = parts[0];
                String password = parts[1];

                var userAccess = _dataAccessor.Authenticate(login, password)
                    ?? throw new Exception("Credintials rejected.");

                _authService.SetAuth(userAccess);
                restResponce.Status = RestStatus.Status200;
                restResponce.Data = new
                {
                    Id = userAccess.UserId,
                    Login = userAccess.Login,
                    User = userAccess.User?.Name,
                    Role = userAccess.Role.Id
                };
                return restResponce;
            }
            catch (Exception ex)
            {
                restResponce.Status = RestStatus.Status400;
                restResponce.Data = new { Error = ex.Message };
                return restResponce;
            }

        }


        private (String, String) GetBasicCredentials()
        {
            String? header = HttpContext.Request.Headers.Authorization;
            if (header == null)      // Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            {
                throw new Exception("Authorization Header Required");
            }
            String credentials =    // 'Basic ' - length = 6
                header[6..];        // QWxhZGRpbjpvcGVuIHNlc2FtZQ==
            String userPass =       // Aladdin:open sesame
                System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(credentials));

            String[] parts = userPass.Split(':', 2);
            String login = parts[0];
            String password = parts[1];
            return (login, password);
        }
    }
}