using Microsoft.AspNetCore.Mvc;
using BackendVisitas.Models;
using Microsoft.Data.SqlClient;
using Serilog;

namespace BackendVisitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private IConfiguration _configuration;
        private string _connectionString;

        public EmployeesController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            this.setupConnectionString("DefaultConnection");
        }

        private void setupConnectionString(string connectionString)
        {
            var connection = _configuration.GetConnectionString(connectionString);
            if (string.IsNullOrEmpty(connection)) {
                Log.Fatal("Missing connection string: " + connectionString + " in appsettings.json.");
                throw new ArgumentException("Missing connection string '" + connectionString + "'.");
            }
            this._connectionString = connection;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            Log.Information("Fetching all employees' information from the database");
            var employees = new List<Employee>();

            using var connection = new SqlConnection(this._connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT *
                FROM Employees
                ORDER BY ID
            ";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var employee = new Employee
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Department = reader.GetString(2)
                };
                employees.Add(employee);
            }
            return employees;
        }
    }
}
