using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mapa_back.Helpers
{
	public class JwtHelper
	{
		private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

		public JwtHelper()
		{
			_jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
		}

		public string GenerateJwtToken(string userId, string role)
		{
			byte[] key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

			List<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Role, role)
			};

			var identity = new ClaimsIdentity(claims);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
				Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
				Subject = identity,
				Expires = DateTime.UtcNow.AddMinutes(30),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			JwtSecurityToken token = _jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);

			return _jwtSecurityTokenHandler.WriteToken(token);
		}

		public ClaimsPrincipal ValidateJwtToken(string token)
		{
			byte[] key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
					ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
					IssuerSigningKey = new SymmetricSecurityKey(key)
				}, out SecurityToken validatedToken);

				return claimsPrincipal;
			}
			catch (SecurityTokenExpiredException)
			{
				throw new ApplicationException("Token has expired.");
			}
		}
	}
}