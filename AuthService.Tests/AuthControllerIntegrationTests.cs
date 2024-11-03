using System.Net.Http.Json;
using AuthService.Data;
using AuthService.Model.Dto;
using AuthService.Model.Entity;
using AuthService.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Tests;

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
            Address = new AddressDto
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsTrue(responseObj?.Success);
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!"
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsTrue(resp.Success);
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Username = "wrong@example.com",
            Password = "WrongPassword!"
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsFalse(resp.Success);
    }

    private async Task SetupDbData()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<ApplicationUser>
                >();
            var roleManager = scope.ServiceProvider.GetRequiredService<
                RoleManager<IdentityRole<int>>
            >();
            // Ensure the roles exist in the database
            string[] roles = { "GUEST", "HOST" };
            int i = 1;
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Id = i, Name = role });
                    i++;
                }
            }

            // Seed the database with a user
            var user = new ApplicationUser
            {
                Id = 1,
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "First Name",
                LastName = "Last Name",
                PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
                ExternalId = Guid.NewGuid(),
                SecurityStamp = Guid.NewGuid().ToString(),
                Address = new Address
                {
                    City = "City",
                    Country = "Country",
                    PostNumber = "PostNumber",
                    StreetName = "StreetName",
                    StreetNumber = "StreetNumber"
                }
            };

            _db.ApplicationUsers.Add(user);
            await _db.SaveChangesAsync();
            await userManager.AddToRolesAsync(user, new[] { "GUEST" });
        }
    }
}
