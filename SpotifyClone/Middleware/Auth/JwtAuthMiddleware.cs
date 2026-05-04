using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpotifyClone.Middleware.Auth
{
    public class JwtAuthMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var secret = configuration["Jwt:Secret"]!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var principal = handler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "ASP-32",
                        ValidAudience = "ASP-32",
                        IssuerSigningKey = key
                    }, out _);

                    context.User = principal;
                }
                catch
                {
                   
                }
            }

            await _next(context);
        }
    }

    public static class JwtAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder builder)
            => builder.UseMiddleware<JwtAuthMiddleware>();
    }
}