using backend.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userService;
        private readonly IMessageService _messageService;
        private readonly IConfiguration _configuration;

        public UserController(
            IUserServices userService,
            IMessageService messageService,
            IConfiguration configuration)
        {
            _userService = userService;
            _messageService = messageService;
            _configuration = configuration;
        }

        public class SignupRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            try
            {
                if (
                    string.IsNullOrEmpty(request.Email) ||
                    string.IsNullOrEmpty(request.Password) ||
                    string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest("All fields are required");
                }

                var existingUser = await _userService.GetUserByEmailAsync(request.Email);

                if (existingUser != null)
                {
                    return Conflict(new { message = "Email address is already in use" });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var user = new User
                {
                    Email = request.Email,
                    Username = request.Username,
                    PasswordHash = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.MinValue
                };

                await _userService.CreateUserAsync(user);

                var emailMessage = new EmailMessage
                {
                    To = user.Email,
                    Username = user.Username,
                    Subject = "Welcome to Youtube Transcriber!",
                    Body = $@"Dear {user.Username},

Welcome to our platform! We're excited to have you on board.

Your account has been successfully created with the following details:
- Email: {user.Email}
- Username: {user.Username}

You can now log in to your account and start using our services.

Best regards,
Youtube Transcriber"
                };

                _messageService.PublishMessage(emailMessage, "email_notifications");

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during registration: {ex.Message}");
                return StatusCode(500, "An error occurred during registration");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || 
                    string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Email and password are required");
                }

                var user = await _userService.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                if (!isValidPassword)
                {
                    return Unauthorized(new { message = "Invalid password" });
                }

                user.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user);

                var tokenstr = GenerateJwtToken(user);


                return Ok(new { 
                    message = "Login successful",
                    userId = user.Id,
                    email = user.Email,
                    token = tokenstr
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { message = "An internal server error occurred" });
            }
        }
    }
}
