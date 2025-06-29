using BackendVisitas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog;

namespace BackendVisitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController: ControllerBase
    {
        private IConfiguration _configuration;
        private string _connectionString;

        public VisitsController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            setupConnectionString("DefaultConnection");
        }

        private void setupConnectionString(string connectionString)
        {
            var connection = _configuration.GetConnectionString(connectionString);
            if (string.IsNullOrEmpty(connection))
            {
                Log.Fatal("At CustomersController, missing connection string: " + connectionString + " in appsettings.json.");
                throw new ArgumentException("Missing connection string '" + connectionString + "'.");
            }
            this._connectionString = connection;
        }

        [HttpPost]
        public async Task<ActionResult> CreateVisit([FromBody] Visit newVisit)
        {
            Log.Information("VisitsController: Creating a new visit");
            
            const string query = @"
                INSERT INTO Visits (EmployeeID, CustomerID, VisitDate)
                VALUES (@EmployeeID, @CustomerID, @VisitDate);
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", newVisit.EmployeeID);
                command.Parameters.AddWithValue("@CustomerID", newVisit.CustomerID);
                command.Parameters.AddWithValue("@VisitDate", newVisit.VisitDate);

                await command.ExecuteNonQueryAsync();
                return Ok("Visit created successfully.");
            } catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500);
            }

        }
    }
}
