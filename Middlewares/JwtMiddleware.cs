using mapa_back.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace mapa_back.Middlewares
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly JwtHelper _jwtSecurityTokenHandler;

        public JwtMiddleware(JwtHelper jwtSecurityTokenHandler)
        {
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the token from the Authorization header
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Verify the token using the JwtSecurityTokenHandlerWrapper
                    var claimsPrincipal = _jwtSecurityTokenHandler.ValidateJwtToken(token);

                    // Extract the user ID and role from the token
                    var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

                    // Store details in the HttpContext items for later use
                    context.Items["UserId"] = userId;
                    context.Items["UserRole"] = userRole;

                    // Set the ClaimsPrincipal User for built-in authorization support
                    context.User = claimsPrincipal;
                }
                catch (Exception)
                {
                    // If the token is invalid, return 401 immediately and stop execution
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }
            // Continue processing the request
            await next(context);
        }
    }
}
