using SpotifyClone.Data.Entities;
using SpotifyClone.Services.Auth;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace SpotifyClone.Middleware.Auth
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;
        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            if(context.Request.Query.ContainsKey("logout"))
            {
                authService.RemoveAuth();
                context.Response.Redirect(context.Request.Path);
                return;
            }
            
            {
                if(authService.GetAuth<UserAccess>() 
                  is UserAccess userAccess)
                {
                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                               new Claim(ClaimTypes.Sid, userAccess.Id.ToString()), 
                               new Claim(ClaimTypes.PrimarySid, userAccess.UserId.ToString()),
                               new Claim(ClaimTypes.Name, userAccess.User.Name),
                               new Claim(ClaimTypes.Role, userAccess.RoleId),
                            ],
                            nameof(SessionAuthMiddleware)
                        )
                    );
                }
                    
            }
            await _next(context);
        }
    }

    public static class SessionAuthMiddlewareExtensions
    { 
        public static IApplicationBuilder UseSessionAuth(
            this IApplicationBuilder builder) 
        { 
            return builder.UseMiddleware<SessionAuthMiddleware>();
        } 
    }
}
