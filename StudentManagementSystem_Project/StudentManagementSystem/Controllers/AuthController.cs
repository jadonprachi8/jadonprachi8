using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.DTOs;
using StudentManagementSystem.DTOs.Auth;
using StudentManagementSystem.Services;

namespace StudentManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ITokenService tokenService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate with username/password and receive a JWT access token.
        /// Credentials are configured under "AdminCredentials" in appsettings.json.
        /// Default (change in appsettings): username "admin", password "Admin@123"
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var configuredUser = _configuration["AdminCredentials:Username"];
            var configuredPass = _configuration["AdminCredentials:Password"];
            if (request.Username != configuredUser || request.Password != configuredPass)
            {
                _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
                return Unauthorized(ApiResponse<object>.FailResponse("Invalid username or password."));
            }

            var (token, expiresAt) = _tokenService.GenerateToken(request.Username);
            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = request.Username
            };

            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful"));
        }
    }
}
