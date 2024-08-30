using System.Net.Http.Json;
using AuthService.Data;
using AuthService.Model.Dto;
using AuthService.Model.Entity;
using AuthService.Model.ServiceResponse;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

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
        [Ignore("")]
        public async Task Register_UserCreatedSuccessfully_ReturnsOk()
        {
            //SetupDbData();
            // Arrange
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "newuser@example.com",
                FirstName = "First Name",
                LastName = "Last Name",
                Password = "Password123!",
                Role = "GUEST",
                Address = new()
                {
                    City="City",
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

        [Test] [Ignore("")]
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
            var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.IsTrue(resp.Success);
        }
        
        [Test] [Ignore("")]
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
            var resp = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.IsFalse(resp.Success);
        }
        
        private async void SetupDbData()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                UserManager<ApplicationUser?> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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
                    Email = "test@example.com", 
                    UserName = "test@example.com",
                    FirstName = "First Name",
                    LastName = "Last Name",
                    PasswordHash = "AQAAAAIAAYagAAAAEBQ7++M6z5N+Tly9yfor8HhJxhg52bNmZAIANR+cR6og/UgoUz8GhnlZQr2NFAP48g==",
                    ExternalId = Guid.NewGuid(),
                    Address = new()
                    {
                        City="City",
                        Country = "Country",
                        PostNumber = "PostNumber",
                        StreetName = "StreetName",
                        StreetNumber = "StreetNumber"
                    }
                };

                _db.ApplicationUsers.Add(user);
                _db.Addresses.Add(new()
                {
                    City = "City",
                    Country = "Country",
                    PostNumber = "PostNumber",
                    StreetName = "StreetName",
                    StreetNumber = "StreetNumber"
                });
                _db.SaveChanges();
                await userManager.AddToRolesAsync(user, new[] { "GUEST" });
            }
        }
    }