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

        public class UserUpdateRequest
        {
            public required string Email { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required int IdRole { get; set; }
            public string? Password { get; set; }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdStr = HttpContext.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }
            User? user = await usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                idRole = user.IdRole
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var userIdStr = HttpContext.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != "1") return Forbid();

            var users = await usersService.GetAllUsersAsync();
            return Ok(users.Select(u => new
            {
                id = u.Id,
                email = u.Email,
                firstName = u.FirstName,
                lastName = u.LastName,
                idRole = u.IdRole
            }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var userIdStr = HttpContext.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != "1") return Forbid();

            User? user = await usersService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Email != request.Email)
            {
                User? existing = await usersService.GetUserByEmailAsync(request.Email);
                if (existing != null) return Conflict("Email jest już zajęty.");
            }

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IdRole = request.IdRole;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));
            }

            var success = await usersService.UpdateUserAsync(user);
            if (!success) return StatusCode(500, "Wystąpił błąd podczas aktualizacji użytkownika.");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userIdStr = HttpContext.Items["UserId"]?.ToString();
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userRole = HttpContext.Items["UserRole"]?.ToString();
            if (userRole != "1") return Forbid();

            if (userIdStr == id.ToString())
            {
                return BadRequest("Nie możesz usunąć samego siebie.");
            }

            var success = await usersService.DeleteUserAsync(id);
            if (!success) return NotFound();

            return Ok();
        }
    }
}
