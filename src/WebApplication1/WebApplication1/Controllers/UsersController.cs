using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Persistant;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(ApplicationDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a user by ID (excluding sensitive data)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        try
        {
            // Using EF Core - no SQL injection possible
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email
                    // Password explicitly excluded
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogInformation("User {UserId} not found", id);
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    // Note: Login should use ASP.NET Core Identity
    // Shown here for demo purposes only
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password required");
        }

        try
        {
            // In production, use ASP.NET Core Identity
            // This is simplified for demo
            var user = await _context.Users
                .Where(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                // Generic message - don't reveal if username exists
                _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
                return Unauthorized("Invalid credentials");
            }

            // In production: generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new LoginResult
            {
                Token = token,
                UserId = user.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Authorization required!
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid user ID");
        }

        try
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted by {AdminUser}",
                id, User.Identity?.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred");
        }
    }

    // Placeholder methods - would use real implementations
    private bool VerifyPassword(string password, string hash) => true;
    private string GenerateJwtToken(User user) => "token";
}

// DTOs
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
}