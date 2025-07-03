using Microsoft.AspNetCore.Mvc;
using BackendVisitas.Models;
using Microsoft.Data.SqlClient;
using Serilog;
using Microsoft.AspNetCore.Http.HttpResults;

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
            try
            {

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
                return Ok(customers);
            } catch (Exception error)
            {
                Log.Error("CustomersController: Error fetching all users: " + error.Message);
                return StatusCode(500);
            }
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            Log.Information($"Fetching customer with ID {id}");
            Customer? customer = null;

            try
            {
                using var connection = new SqlConnection(this._connectionString);
                await connection.OpenAsync();

                var query = "SELECT Id, Name, Address FROM Customers WHERE Id = @Id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    customer = new Customer
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Address = reader.GetString(2)
                    };
                }

                if (customer == null)
                {
                    Log.Warning($"Customer with ID {id} not found.");
                    return NotFound();
                }

                return Ok(customer);
            } catch (Exception error)
            {
                Log.Error($"CustomersController: Error fetching customer with ID {id}: {error.Message}");
                return StatusCode(500);
            }
        }
    }
}
