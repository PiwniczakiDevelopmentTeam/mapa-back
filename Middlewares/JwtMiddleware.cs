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
			string token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
			if (!string.IsNullOrEmpty(token))
			{
				try
				{
					ClaimsPrincipal claimsPrincipal = _jwtSecurityTokenHandler.ValidateJwtToken(token);

					string? userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					string? userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

					context.Items["UserId"] = userId;
					context.Items["UserRole"] = userRole;

					context.User = claimsPrincipal;
				}
				catch (Exception)
				{
					context.Response.StatusCode = 401;
					await context.Response.WriteAsync("Unauthorized");
					return;
				}
			}
			Endpoint? endpoint = context.GetEndpoint();
			if (endpoint != null)
			{
				Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
				if (actionDescriptor != null)
				{
					Microsoft.AspNetCore.Authorization.IAllowAnonymous? allowAnonymous = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>();
					if (allowAnonymous == null)
					{
						if (context.Items["UserId"] == null)
						{
							context.Response.StatusCode = 401;
							await context.Response.WriteAsync("Unauthorized");
							return;
						}
					}
				}
			}

			await next(context);
		}
	}
}