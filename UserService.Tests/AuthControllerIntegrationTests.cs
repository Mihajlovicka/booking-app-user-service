using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UserService.Data;
using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Tests;

[TestFixture]
[Category("Integration")]
public class AuthControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        SetupDbData().Wait();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsOk()
    {
        var registrationRequest = new RegistrationRequestDto
        {
            Email = "newuser@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            Password = "Password123!",
            Role = "GUEST",
            Address = new AddressDto()
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber",
            },
        };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequest);

        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!",
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.EnsureSuccessStatusCode();

        var responseObj = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.That(responseObj?.Token, Is.Not.Empty);
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "wrong@example.com",
            Password = "WrongPassword!",
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SetupDbData()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            UserManager<ApplicationUser?> userManager = scope.ServiceProvider.GetRequiredService<
                UserManager<ApplicationUser>
            >();
            var roleManager = scope.ServiceProvider.GetRequiredService<
                RoleManager<IdentityRole<int>>
            >();

            // Ensure the roles exist in the database
            string[] roles = { "GUEST", "HOST" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }
            // Seed the database with a user
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "First Name",
                LastName = "Last Name",
                PasswordHash =
                    "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
                ExternalId = Guid.NewGuid(),
                SecurityStamp = Guid.NewGuid().ToString(),
                Address = new Address()
                {
                    City = "City",
                    Country = "Country",
                    PostNumber = "PostNumber",
                    StreetName = "StreetName",
                    StreetNumber = "StreetNumber",
                },
            };

            _db.ApplicationUsers.Add(user);
            _db.Addresses.Add(
                new()
                {
                    City = "City",
                    Country = "Country",
                    PostNumber = "PostNumber",
                    StreetName = "StreetName",
                    StreetNumber = "StreetNumber",
                }
            );
            _db.SaveChanges();
            await userManager.AddToRolesAsync(user, new[] { "GUEST" });
        }
    }
}
