using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using UserService.Data;
using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Tests;

[TestFixture]
[Category("Integration")]
public class UserControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        await SetupDbData();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetLogged_AuthenticatedUser_ReturnsOk()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!",
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseObj = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResponseObj?.Token
            );

        var response = await _client.GetAsync("/api/user");

        response.EnsureSuccessStatusCode();

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.AreEqual("test@example.com", userDto.Email);
    }

    [Test]
    public async Task Update_AuthenticatedUser_ReturnsOk()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!",
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseObj = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResponseObj?.Token
            );

        var updateUserRequest = new UserDto
        {
            Id = loginResponseObj.User.Id,
            Email = "test@example.com",
            Username = "test@example.com",
            FirstName = "Updated First Name",
            LastName = "Updated Last Name",
            Address = new AddressDto
            {
                City = "Updated City",
                Country = "Updated Country",
                PostNumber = "Updated PostNumber",
                StreetName = "Updated StreetName",
                StreetNumber = "Updated StreetNumber",
            },
        };

        var response = await _client.PostAsJsonAsync("/api/user", updateUserRequest);

        response.EnsureSuccessStatusCode();

        var updatedUserDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(updatedUserDto);
        Assert.AreEqual(updateUserRequest.FirstName, updatedUserDto.FirstName);
        Assert.AreEqual(updateUserRequest.LastName, updatedUserDto.LastName);
        Assert.AreEqual(updateUserRequest.Address.City, updatedUserDto.Address.City);
    }

    [Test]
    public async Task Update_UserNotFound_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!",
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseObj = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResponseObj?.Token
            );

        var updateUserRequest = new UserDto
        {
            Id = Guid.NewGuid(), // Non-existent user ID
            Email = "nonexistent@example.com",
            FirstName = "Nonexistent",
            LastName = "User",
            Role = "GUEST",
            Address = new AddressDto
            {
                City = "Nonexistent City",
                Country = "Nonexistent Country",
                PostNumber = "Nonexistent PostNumber",
                StreetName = "Nonexistent StreetName",
                StreetNumber = "Nonexistent StreetNumber",
            },
        };

        var response = await _client.PostAsJsonAsync("/api/user", updateUserRequest);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public async Task Update_EmailAlreadyExists_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Username = "test@example.com",
            Password = "Password123!",
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseObj = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                loginResponseObj?.Token
            );

        var updateUserRequest = new UserDto
        {
            Id = loginResponseObj.User.Id,
            Email = "existing@example.com", // Email that already exists in the database
            FirstName = "Updated First Name",
            LastName = "Updated Last Name",
            Role = "GUEST",
            Address = new AddressDto
            {
                City = "Updated City",
                Country = "Updated Country",
                PostNumber = "Updated PostNumber",
                StreetName = "Updated StreetName",
                StreetNumber = "Updated StreetNumber",
            },
        };

        var response = await _client.PostAsJsonAsync("/api/user", updateUserRequest);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SetupDbData()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<
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
                Address = new Address
                {
                    City = "City",
                    Country = "Country",
                    PostNumber = "PostNumber",
                    StreetName = "StreetName",
                    StreetNumber = "StreetNumber",
                },
            };

            var existingUser = new ApplicationUser
            {
                Email = "existing@example.com",
                UserName = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                PasswordHash =
                    "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
                ExternalId = Guid.NewGuid(),
                SecurityStamp = Guid.NewGuid().ToString(),
                Address = new Address
                {
                    City = "Existing City",
                    Country = "Existing Country",
                    PostNumber = "Existing PostNumber",
                    StreetName = "Existing StreetName",
                    StreetNumber = "Existing StreetNumber",
                },
            };

            _db.ApplicationUsers.Add(user);
            _db.ApplicationUsers.Add(existingUser);
            _db.Addresses.Add(user.Address);
            _db.Addresses.Add(existingUser.Address);
            _db.SaveChanges();
            await userManager.AddToRolesAsync(user, new[] { "GUEST" });
            await userManager.AddToRolesAsync(existingUser, new[] { "GUEST" });
        }
    }
}
