using AuthService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace AuthService.Tests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private string _connectionString;
    private string _databaseName;

    public CustomWebApplicationFactory()
    {
        var baseConnectionString = "Server=db;Port=3306;Database=booking-auth;Uid=root;Pwd=admin;";
        // var baseConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database") 
        //                            ?? "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=admin;";

        _databaseName = $"test_{Guid.NewGuid()}";
        
        var builder = new MySqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _databaseName
        };
        _connectionString = builder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            var baseConnectionString = new MySqlConnectionStringBuilder(_connectionString) { Database = "" }.ConnectionString;

            using var connection = new MySqlConnection(baseConnectionString);
            connection.Open();

            using var command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{_databaseName}`;", connection);
            command.ExecuteNonQuery();

            services.AddDbContext<AppDbContext>(options => options.UseMySQL(_connectionString));

            var dbContext = CreateDbContext(services);
            dbContext.Database.EnsureDeleted();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var baseConnectionString = new MySqlConnectionStringBuilder(_connectionString) { Database = "" }.ConnectionString;

            using var connection = new MySqlConnection(baseConnectionString);
            connection.Open();

            using var command = new MySqlCommand($"DROP DATABASE IF EXISTS `{_databaseName}`;", connection);
            command.ExecuteNonQuery();
        }

        base.Dispose(disposing);
    }

    public AppDbContext CreateDbContext(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
