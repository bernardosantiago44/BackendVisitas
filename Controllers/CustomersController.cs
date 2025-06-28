using Microsoft.AspNetCore.Mvc;
using BackendVisitas.Models;
using Microsoft.Data.SqlClient;
using Serilog;

namespace BackendVisitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private IConfiguration _configuration;
        private string _connectionString;

        public CustomersController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            this.setupConnectionString("DefaultConnection");
        }

        private void setupConnectionString(string connectionString)
        {
            var connection = _configuration.GetConnectionString(connectionString);
            if (string.IsNullOrEmpty(connection)) {
                Log.Fatal("At CustomersController, missing connection string: " + connectionString + " in appsettings.json.");
                throw new ArgumentException("Missing connection string '" + connectionString + "'.");
            }
            this._connectionString = connection;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            Log.Information("Fetching all customers' information from the database");
            var customers = new List<Customer>();

            using var connection = new SqlConnection(this._connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT *
                FROM Customers
                ORDER BY ID
            ";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var customer = new Customer
                {
                    Id = reader.GetInt32(2),
                    Name = reader.GetString(0),
                    Address = reader.GetString(1)
                };
                customers.Add(customer);
            }
            return customers;
        }
    }
}
