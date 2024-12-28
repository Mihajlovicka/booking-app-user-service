using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Data;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Service.MessagingService;

namespace UserService.Tests;

[TestFixture]
[Category("Integration")]
public class AuthControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;
    private string KafkaBroker = "localhost:9092";

    [OneTimeSetUp]
    public async Task Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        SetupDbData().Wait();
        var config = _factory.Services.GetRequiredService<IConfiguration>();

        // Access Kafka configuration
        KafkaBroker = config.GetValue<string>("KafkaConfig:Producer:BootstrapServers");
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

        var message = await ConsumeKafkaMessage(KafkaTopic.UserCreated.ToString());
        Assert.NotNull(message, "No message received from Kafka");

        var userDto = JsonSerializer.Deserialize<UserDto>(message);
        Assert.NotNull(userDto, "Message format is incorrect");
        Assert.AreEqual(registrationRequest.Email, userDto.Email);
        Assert.AreEqual(registrationRequest.Role, userDto.Role);
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

    private async Task<string> ConsumeKafkaMessage(string topic)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = KafkaBroker,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        var timeout = TimeSpan.FromSeconds(10);
        var start = DateTime.Now;

        try
        {
            while (DateTime.Now - start < timeout)
            {
                var result = consumer.Consume(100);
                if (result != null && !string.IsNullOrWhiteSpace(result.Message?.Value))
                {
                    return result.Message.Value;
                }
            }
        }
        finally
        {
            consumer.Close();
        }
        return null; // Timeout if no message is received
    }
}
