using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.API.Data;
using ITAssetManagement.API.Models;
using ITAssetManagement.API.Services;
using ITAssetManagement.API.Dtos;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt for username: {request.Username}");
                
                // Find user by username first (don't check IsActive yet for debugging)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
                
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {request.Username}");
                    return Unauthorized(new { message = "Invalid credentials", debug = "User not found" });
                }
                
                _logger.LogInformation($"User found: {user.Username}, IsActive: {user.IsActive}");
                _logger.LogInformation($"Stored password starts with: {user.Password.Substring(0, Math.Min(10, user.Password.Length))}...");
                _logger.LogInformation($"Provided password: {request.Password}");
                
                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning($"User is inactive: {request.Username}");
                    return Unauthorized(new { message = "Invalid credentials", debug = "User inactive" });
                }
                
                // Check if password looks like a BCrypt hash
                bool looksLikeBCrypt = user.Password.StartsWith("$2a$") || user.Password.StartsWith("$2b$") || user.Password.StartsWith("$2y$");
                _logger.LogInformation($"Password looks like BCrypt: {looksLikeBCrypt}");
                
                bool passwordValid = false;
                
                if (looksLikeBCrypt)
                {
                    try
                    {
                        // BCrypt hashed password
                        passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                        _logger.LogInformation($"BCrypt verification result: {passwordValid}");
                    }
                    catch (Exception bcryptEx)
                    {
                        _logger.LogError(bcryptEx, "BCrypt verification failed with exception");
                        return Unauthorized(new { message = "Invalid credentials", debug = "BCrypt error" });
                    }
                }
                else
                {
                    // Fall back to plain text comparison for debugging
                    passwordValid = user.Password == request.Password;
                    _logger.LogInformation($"Plain text comparison result: {passwordValid}");
                }
                
                if (!passwordValid)
                {
                    _logger.LogWarning($"Password verification failed for user: {request.Username}");
                    return Unauthorized(new { message = "Invalid credentials", debug = "Password mismatch" });
                }

                var token = _jwtService.GenerateToken(user);
                _logger.LogInformation($"Login successful for user: {request.Username}");

                return Ok(new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for username: {request.Username}");
                return StatusCode(500, new { message = "Internal server error", debug = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }
        
        // Add this temporary endpoint for debugging
        [HttpGet("debug-user/{username}")]
        public async Task<IActionResult> DebugUser(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound("User not found");
            
            return Ok(new {
                username = user.Username,
                passwordLength = user.Password.Length,
                passwordPrefix = user.Password.Substring(0, Math.Min(15, user.Password.Length)),
                isActive = user.IsActive,
                role = user.Role,
                looksLikeBCrypt = user.Password.StartsWith("$2a$") || user.Password.StartsWith("$2b$") || user.Password.StartsWith("$2y$")
            });
        }
    }
}