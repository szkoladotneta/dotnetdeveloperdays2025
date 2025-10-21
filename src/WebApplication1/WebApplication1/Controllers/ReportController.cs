using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IConfiguration _config;

    public ReportController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("sales")]
    public IActionResult GetSalesReport(string startDate, string endDate)
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        var conn = new SqlConnection(connStr);
        conn.Open();

        var query = $"SELECT * FROM Sales WHERE Date >= '{startDate}' AND Date <= '{endDate}'";
        var cmd = new SqlCommand(query, conn);

        var reader = cmd.ExecuteReader();
        var results = new List<object>();

        while (reader.Read())
        {
            results.Add(new
            {
                date = reader["Date"],
                amount = reader["Amount"]
            });
        }

        return Ok(results);
    }
}