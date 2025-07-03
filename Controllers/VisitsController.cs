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

        [HttpPost("create")]
        public async Task<ActionResult> CreateVisit([FromBody] Visit newVisit)
        {
            Log.Information("VisitsController: Creating a new visit");
            
            const string query = @"
                INSERT INTO Visits (EmployeeID, CustomerID, VisitDate)
                VALUES (@EmployeeID, @CustomerID, @VisitDate);
                SELECT SCOPE_IDENTITY();
            ";

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@EmployeeID", newVisit.EmployeeID);
                command.Parameters.AddWithValue("@CustomerID", newVisit.CustomerID);
                command.Parameters.AddWithValue("@VisitDate", newVisit.VisitDate);

                var createdId = Convert.ToInt32(await command.ExecuteScalarAsync());
                var createdVisit = new Visit
                {
                    Id = createdId,
                    EmployeeID = newVisit.EmployeeID,
                    CustomerID = newVisit.CustomerID,
                    VisitDate = newVisit.VisitDate
                };

                return Ok(createdVisit);
            } catch (Exception error)
            {
                Log.Error(error.Message);
                return StatusCode(500);
            }

        }

        [HttpPost("all")]
        public async Task<ActionResult<IEnumerable<Visit>>> GetAll()
        {
            Log.Information("VisitsController: Fetching all visits.");

            var visits = new List<Visit>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT * 
                    FROM Visits
                    ORDER BY ID
                ";
                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    Visit visit = new Visit
                    {
                        Id = reader.GetInt32(0),
                        CustomerID = reader.GetInt32(1),
                        EmployeeID = reader.GetInt32(2),
                        VisitDate = reader.GetDateTime(3)
                    };
                    visits.Add(visit);
                }

                return Ok(visits);
            } catch (Exception error)
            {
                Log.Error(error.Message);
                return StatusCode(500);
            }
        }
    }
}
