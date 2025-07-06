using BackendVisitas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllAsync();
}


namespace BackendVisitas.Controllers
{
    public class EmployeeService : ControllerBase, IEmployeeService
    {
        private IConfiguration _configuration;
        private string _connectionString;
        public EmployeeService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            this.setupConnectionString("DefaultConnection");
        }

        private void setupConnectionString(string connectionName)
        {
            var connection = _configuration.GetConnectionString(connectionName);
            if (string.IsNullOrEmpty(connection))
            {
                Log.Fatal($"EmployeeService.setupConnectionString - Could not find ${connectionName} in appsettings.json");
            }
            this._connectionString = connection!;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            Log.Information("Fetching all employees' information from the database");
            var employees = new List<Employee>();

            try
            {
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
            }
            catch (Exception error)
            {
                Log.Error($"EmployeesService.GetAllAsync: Error fetching all employees: {error.Message}");
            }
            return employees;
        }

    }
}
