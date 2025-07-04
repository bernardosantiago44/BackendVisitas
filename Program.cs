using BackendVisitas.Controllers;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using Serilog;

namespace BackendVisitas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            Log.Information("Backend Visitas - Bernardo Santiago");

            ExcelPackage.License.SetNonCommercialPersonal("Bernardo Santiago Marin");

            // Add CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddTransient<SqlConnection>(_ => 
            new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<IVisitService, VisitService>();
            builder.Services.AddScoped<ICustomersService, CustomersService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors("AllowAll");

            app.UseStaticFiles();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Employees}/{action=GetAll}");

            app.Run();
        }
    }
}
