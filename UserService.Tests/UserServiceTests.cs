using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using UserService.Mapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Service.Contract;
using UserService.Service.Implementation;
using UserService.Service.MessagingService;

namespace UserService.Tests;

[TestFixture]
[Category("Unit")]
public class UserServiceTests
{
    private Mock<IUserContextService> _mockUserContextService;
    private Mock<IMapperManager> _mockMapperManager;
    private Mock<IRepositoryManager> _mockRepositoryManager;
    private Mock<ProducerService> _mockProducerService;
    private IUserService _userService;

    [SetUp]
    public void Setup()
    {
        _mockUserContextService = new Mock<IUserContextService>();
        _mockMapperManager = new Mock<IMapperManager>();
        _mockRepositoryManager = new Mock<IRepositoryManager>();

        var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
        var mockKafkaConfig = Mock.Of<IOptions<ProducerConfig>>(options =>
            options.Value == producerConfig
        );

        _mockProducerService = new Mock<ProducerService>(
            new Mock<ILogger<ProducerService>>().Object,
            mockKafkaConfig
        );

        _userService = new Service.Implementation.UserService(
            _mockUserContextService.Object,
            _mockMapperManager.Object,
            _mockRepositoryManager.Object,
            _mockProducerService.Object
        );

        _mockProducerService.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<object>()));
    }

    [Test]
    public async Task Update_UserExists_ReturnsUpdatedUserDto()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "updated@example.com",
            Username = "updatedusername",
            Address = new AddressDto
            {
                City = "UpdatedCity",
                Country = "UpdatedCountry",
                PostNumber = "UpdatedPostNumber",
                StreetName = "UpdatedStreetName",
                StreetNumber = "UpdatedStreetNumber",
            },
        };

        var user = new ApplicationUser
        {
            ExternalId = userDto.Id,
            FirstName = "FirstName",
            LastName = "LastName",
            Email = "example@example.com",
            UserName = "username",
            Address = new Address
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber",
            },
        };

        _mockRepositoryManager
            .Setup(r => r.UserRepository.GetByExternalId(userDto.Id))
            .ReturnsAsync(user);

        _mockRepositoryManager
            .Setup(r => r.UserRepository.UserNameExists(userDto.Id, userDto.Username))
            .ReturnsAsync((ApplicationUser)null);

        _mockRepositoryManager
            .Setup(r => r.UserRepository.EmailExists(userDto.Id, userDto.Email))
            .ReturnsAsync((ApplicationUser)null);

        _mockMapperManager
            .Setup(m => m.ApplicationUserToUserDtoMapper.Map(It.IsAny<ApplicationUser>()))
            .Returns(userDto);

        // Act
        var result = await _userService.Update(userDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.FirstName, Is.EqualTo(userDto.FirstName));
            Assert.That(result.LastName, Is.EqualTo(userDto.LastName));
            Assert.That(result.Email, Is.EqualTo(userDto.Email));
            Assert.That(result.Username, Is.EqualTo(userDto.Username));
            Assert.That(result.Address.City, Is.EqualTo(userDto.Address.City));
            Assert.That(result.Address.Country, Is.EqualTo(userDto.Address.Country));
            Assert.That(result.Address.PostNumber, Is.EqualTo(userDto.Address.PostNumber));
            Assert.That(result.Address.StreetName, Is.EqualTo(userDto.Address.StreetName));
            Assert.That(result.Address.StreetNumber, Is.EqualTo(userDto.Address.StreetNumber));
        });
    }

    [Test]
    public void Update_UserNotFound_ThrowsBadHttpRequestException()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Email = "example@example.com",
            Username = "username",
        };

        _mockRepositoryManager
            .Setup(r => r.UserRepository.GetByExternalId(userDto.Id))
            .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _userService.Update(userDto)
        );
        Assert.That(ex.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public void Update_UsernameExists_ThrowsBadHttpRequestException()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Email = "example@example.com",
            Username = "username",
        };

        var existingUser = new ApplicationUser
        {
            ExternalId = Guid.NewGuid(),
            UserName = "existingusername",
        };

        _mockRepositoryManager
            .Setup(r => r.UserRepository.GetByExternalId(userDto.Id))
            .ReturnsAsync(new ApplicationUser());

        _mockRepositoryManager
            .Setup(r => r.UserRepository.UserNameExists(userDto.Id, userDto.Username))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _userService.Update(userDto)
        );
        Assert.That(ex.Message, Is.EqualTo("Username already exists"));
    }

    [Test]
    public void Update_EmailExists_ThrowsBadHttpRequestException()
    {
        // Arrange
        var userDto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = "FirstName",
            LastName = "LastName",
            Email = "example@example.com",
            Username = "username",
        };

        var existingUser = new ApplicationUser
        {
            ExternalId = Guid.NewGuid(),
            Email = "existing@example.com",
        };

        _mockRepositoryManager
            .Setup(r => r.UserRepository.GetByExternalId(userDto.Id))
            .ReturnsAsync(new ApplicationUser());

        _mockRepositoryManager
            .Setup(r => r.UserRepository.EmailExists(userDto.Id, userDto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _userService.Update(userDto)
        );
        Assert.That(ex.Message, Is.EqualTo("Email already exists"));
    }
}
