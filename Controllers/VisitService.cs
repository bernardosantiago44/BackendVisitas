using BackendVisitas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog;

public interface IVisitService
{
    Task<IEnumerable<Visit>> GetAllAsync();
}

namespace BackendVisitas.Controllers
{
    class VisitService : ControllerBase, IVisitService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public VisitService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = string.Empty;
            try
            {
                setupConnectionString("DefaultConnection");
            }
            catch (Exception error)
            {
                Log.Fatal($"VisitService - ${error.Message}");
            }
        }

        private void setupConnectionString(string connectionName)
        {
            var connection = _configuration.GetConnectionString(connectionName);
            if (string.IsNullOrEmpty(connection))
            {
                throw new Exception("Could not find connection string in appsetting.json.");
            }
            this._connectionString = connection;
        }

        public async Task<IEnumerable<Visit>> GetAllAsync()
        {
            Log.Information("VisitsController: Fetching all visits.");

            var visits = new List<Visit>([]);

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
            }
            catch (Exception error)
            {
                Log.Error($"VisitService - ${error.Message}");
            }
            return visits;

        }
    }
}
