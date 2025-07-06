using BackendVisitas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using Serilog;

namespace BackendVisitas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController: ControllerBase
    {
        private IConfiguration _configuration;
        private readonly IVisitService _visitService;
        private readonly ICustomersService _customersService;
        private readonly IEmployeeService _employeeService;
        private string _connectionString;

        public VisitsController(IConfiguration configuration, 
                                IVisitService visitService,
                                ICustomersService customerService,
                                IEmployeeService employeeService)
        {
            this._connectionString = string.Empty;
            this._configuration = configuration;
            this._visitService = visitService;
            this._customersService = customerService;
            this._employeeService = employeeService;
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
        public async Task<ActionResult<IEnumerable<Visit>>> GetAllAsync()
        {
            Log.Information("VisitsController: Fetching all visits.");

            var visits = await _visitService.GetAllAsync();
            if (visits == null)
            {
                return StatusCode(500);
            }
            return Ok(visits);
        }

        [HttpPost("report")]
        public async Task<IActionResult> GenerateVisitsReport()
        {
            Log.Information("VisitsController - Generating visits report");
            IEnumerable<Visit> visits = await _visitService.GetAllAsync();
            IEnumerable<Customer> customers = await _customersService.GetAllAsync();
            IEnumerable<Employee> employees = await _employeeService.GetAllAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Visits");

            // Headers
            worksheet.Cells[1, 1].Value = "Visit ID";
            worksheet.Cells[1, 2].Value = "Customer";
            worksheet.Cells[1, 3].Value = "Employee";
            worksheet.Cells[1, 4].Value = "Date";

            // Data rows
            for (int i = 0; i < visits.Count(); i++) 
            {
                var visit = visits.ElementAt(i);

                var customer = Array.Find(customers.ToArray(), customer => customer.Id == visit.CustomerID);
                var employee = Array.Find(employees.ToArray(), employee => employee.Id == visit.EmployeeID);

                worksheet.Cells[i + 2, 1].Value = visit.Id;
                worksheet.Cells[i + 2, 2].Value = customer?.Name ?? visit.CustomerID.ToString();
                worksheet.Cells[i + 2, 3].Value = employee?.Name ?? visit.EmployeeID.ToString();
                worksheet.Cells[i + 2, 4].Value = visit.VisitDate.ToString("yyyy-MM-dd");
            }

            worksheet.Cells.AutoFitColumns();

            // Return the Excel file as a downloadable response
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = "VisitsReport.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(stream, contentType, fileName);
        }

    }
}
