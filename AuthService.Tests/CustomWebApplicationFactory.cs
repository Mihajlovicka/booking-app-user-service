using AuthService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;

namespace AuthService.Tests;
internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringTemplate = "Server=localhost;Database={0};Uid=root;Pwd=admin;";
    private string _connectionString;
    private string _databaseName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            _databaseName = $"TestDb_{Guid.NewGuid()}";
            _connectionString = string.Format(ConnectionStringTemplate, _databaseName);

            using (var connection = new MySqlConnection(_connectionString.Replace(_databaseName, "mysql")))
            {
                connection.Open();
                using (var command = new MySqlCommand($"CREATE DATABASE `{_databaseName}`;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySQL(_connectionString));

            var dbContext = CreateDbContext(services);
            dbContext.Database.EnsureDeleted();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (var connection = new MySqlConnection(_connectionString.Replace(_databaseName, "mysql")))
            {
                connection.Open();
                using (var command = new MySqlCommand($"DROP DATABASE IF EXISTS `{_databaseName}`;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        base.Dispose(disposing);
    }

    public AppDbContext CreateDbContext(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetService<AppDbContext>();
    }
}