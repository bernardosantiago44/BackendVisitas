using Microsoft.Data.SqlClient;
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

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddTransient<SqlConnection>(_ => 
            new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

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
