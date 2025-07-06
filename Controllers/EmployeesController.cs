using Microsoft.AspNetCore.Mvc;
using BackendVisitas.Models;
using Microsoft.Data.SqlClient;
using Serilog;
using Microsoft.IdentityModel.Tokens;

namespace BackendVisitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private IConfiguration _configuration;
        private IEmployeeService _employeeService;
        private string _connectionString;

        public EmployeesController(IConfiguration configuration, IEmployeeService employeeService)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            this._employeeService = employeeService;
            this.setupConnectionString("DefaultConnection");
        }

        private void setupConnectionString(string connectionString)
        {
            var connection = _configuration.GetConnectionString(connectionString);
            if (string.IsNullOrEmpty(connection)) {
                Log.Fatal("At EmployeesController, missing connection string: " + connectionString + " in appsettings.json.");
                throw new ArgumentException("Missing connection string '" + connectionString + "'.");
            }
            this._connectionString = connection;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();

            if (employees.IsNullOrEmpty())
            {
                return StatusCode(500);
            }

            return Ok(employees);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {
            Log.Information($"Fetching employee with ID {id}");
            Employee? employee = null;

            try
            {
                using var connection = new SqlConnection(this._connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Department FROM Employees WHERE Id = @Id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    employee = new Employee
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Department = reader.GetString(2)
                    };
                }

                if (employee == null)
                {
                    Log.Warning($"Employee with ID {id} not found.");
                    return NotFound();
                }

                return Ok(employee);
            } catch (Exception error)
            {
                Log.Error($"EmployeesController: Error fetching employee with id ${id}: {error.Message}");
                return StatusCode(500, error);
            }
        }
    }
}
