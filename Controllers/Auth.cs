using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using prakt8_leshukov.Models;
using prakt8_leshukov.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace prakt8_leshukov.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class Auth : ControllerBase
    {
        private readonly List<User> _users;
        private LeshukovDbContext _db = DBService.Instance.Context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _cofiguration;
       
        public Auth(
            List<User> users, IPasswordHasher<User> hasher, IConfiguration cofiguration)
        {
            _users = users;
            _hasher = hasher;
            _cofiguration = cofiguration;

            if(_users.Count == 0)
            {
                _users.AddRange(_db.Users.ToList());
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cofiguration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:"prakt8_leshukov",
                audience: "prakt8_leshukovClient",
                claims:claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials:credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public record AuthRequest (string Login, string Password);
        [HttpPost("register")]
        public IActionResult Register(AuthRequest request)
        {
            if (_users.Any(u => u.Login == request.Login))
                return Conflict("Логин уже существует");

            var user = new User { Login = request.Login };
            if(user.Login == "admin123")
            {
                user.Role = "Admin";
            }
            else
            {
                user.Role = "User";

            }
                
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            _users.Add(user);
            _db.Users.Add(user);
            _db.SaveChanges();
            return Ok("Пользователь зарегестрирован");
        }
        [HttpPost("login")]
        public IActionResult Login(AuthRequest request)
        {
            var user = _users.Find(u => u.Login == request.Login);
            if(user == null)
            {
                return Conflict("Неверная пара логин/пароль");
            }
            var result = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
                );
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Неверная пара логин/пароль");
            var token = GenerateJwtToken(user);
            return Ok(new
            {
                access_token = token

            });
        }
       

    }
}
