using mapa_back.Data;
using mapa_back.Enums;
using mapa_back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace mapa_back.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsersService usersService;
        private SHA256 sha256;
        private JwtHelper jwt;

        public UserController(IUsersService usersService)
        {
            sha256 = SHA256.Create();
            jwt = new JwtHelper();
            this.usersService = usersService;
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            User user = await usersService.GetUserByEmailAsync(request.Email);
            if (user != null)
            {
                if (user.Password == Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password))))
                {
                    string token = jwt.GenerateJwtToken(user.Id.ToString(), user.IdRole.ToString());
                    return Ok(token);
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            User user = await usersService.GetUserByEmailAsync(request.Email);
            string passHash = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));
            if (user == null)
            {
                user = new User()
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Password = passHash,
                    IdRole = (int)Role.User
                };
                await usersService.PostSingleUser(user);
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }
    }
}
