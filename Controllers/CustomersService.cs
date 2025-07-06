using BackendVisitas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog;

public interface ICustomersService
{
    Task<IEnumerable<Customer>> GetAllAsync();
}

namespace BackendVisitas.Controllers
{
    public class CustomersService : ControllerBase, ICustomersService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;
        public CustomersService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            try
            {
                this.setupConnectionString("DefaultConnection");
            } catch (Exception error)
            {
                Log.Fatal($"CustomersService - ${error.Message}");
            }
        }

        private void setupConnectionString(string connectionName)
        {
            var connection = _configuration.GetConnectionString(connectionName);
            if (string.IsNullOrEmpty(connection))
            {
                throw new Exception("Could not find connection string in appsettings.json"); 
            }
            _connectionString = connection;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
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
            }
            catch (Exception error)
            {
                Log.Error("CustomerService: Error fetching all users: " + error.Message);
                
            }
            return customers;
        }
    }
}
