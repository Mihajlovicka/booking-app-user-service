using System.Net.Http.Json;
using System.Text;
using AuthService.Data;
using AuthService.Model;
using AuthService.Model.Dto;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mysqlx.Resultset;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using Response = AuthService.Model.Response;

namespace AuthService.Tests;

public class AuthControllerIntegrationTests
    {
        private HttpClient _client;
        private CustomWebApplicationFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Register_UserCreatedSuccessfully_ReturnsOk()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "newuser@example.com",
                Name = "New User",
                PhoneNumber = "1234567890",
                Password = "Password123!",
                Role = "GUEST"
            };
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseObj = await response.Content.ReadFromJsonAsync<Response>();
            Assert.IsTrue(responseObj?.IsSuccess);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            SetupDbData();
            var loginRequest = new LoginRequestDto
            {
                Username = "test@example.com",
                Password = "Password123!"
            };
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
            // Assert
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadFromJsonAsync<Response>();
            Assert.IsTrue(resp.IsSuccess);
        }
        
        [Test]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            SetupDbData();
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
            var resp = await response.Content.ReadFromJsonAsync<Response>();
            Assert.IsFalse(resp.IsSuccess);
        }
        
        private async void SetupDbData()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure the roles exist in the database
                string[] roles = { "GUEST", "HOST" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                // Seed the database with a user
                var user = new ApplicationUser
                {
                    Email = "test@example.com", // Assuming this email already exists
                    Name = "Existing User",
                    UserName = "test@example.com",
                    PhoneNumber = "1234567890",
                    PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g=="
                };

                _db.ApplicationUsers.Add(user);
                _db.SaveChanges();
                await userManager.AddToRolesAsync(user, new[] { "GUEST" });
            }
        }
    }