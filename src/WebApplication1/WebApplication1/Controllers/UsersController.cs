using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(string id)
    {
        // Get connection string from config
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            
            // Build SQL query - PROBLEM: SQL Injection!
            var query = "SELECT UserId, Username, Email, Password FROM Users WHERE UserId = '" + id + "'";
            
            var command = new SqlCommand(query, connection);
            var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                // PROBLEM: Returning password!
                var user = new
                {
                    Id = reader["UserId"],
                    Username = reader["Username"],
                    Email = reader["Email"],
                    Password = reader["Password"] // Should never return this!
                };
                
                return Ok(user);
            }
            
            return NotFound();
        }
    }

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            
            // PROBLEM: SQL Injection + plain text password comparison
            var query = "SELECT * FROM Users WHERE Username = '" + username + 
                       "' AND Password = '" + password + "'";
            
            var command = new SqlCommand(query, connection);
            var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                // PROBLEM: No authentication token, just returning user data
                return Ok(new { message = "Login successful", userId = reader["UserId"] });
            }
            
            // PROBLEM: Revealing whether username exists
            return Unauthorized("Invalid username or password");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(string id)
    {
        // PROBLEM: No authorization check!
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            
            // PROBLEM: SQL Injection again
            var query = "DELETE FROM Users WHERE UserId = '" + id + "'";
            var command = new SqlCommand(query, connection);
            
            // PROBLEM: No error handling
            command.ExecuteNonQuery();
            
            return Ok("User deleted");
        }
    }
}