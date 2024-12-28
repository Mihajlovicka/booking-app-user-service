using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Data;
using UserService.Mapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Service.Contract;
using UserService.Service.MessagingService;

namespace UserService.Tests;

[TestFixture]
[Category("Unit")]
public class Tests
{
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<ApplicationUser>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private Mock<ProducerService> _mockProducerService;
    private IAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _mapperManagerMock = new Mock<IMapperManager>();
        // var loggerMock = new Mock<ILogger<ProducerService>>();
        // var kafkaConfigMock = Mock.Of<IOptions<ProducerConfig>>(x => x.Value == new ProducerConfig());

        // _mockProducerService = new Mock<ProducerService>(loggerMock.Object, kafkaConfigMock);

        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();

        _repositoryManagerMock
            .Setup(repo => repo.UserRepository)
            .Returns(new Mock<IUserRepository>().Object);

        var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
        var mockKafkaConfig = Mock.Of<IOptions<ProducerConfig>>(options =>
            options.Value == producerConfig
        );

        _mockProducerService = new Mock<ProducerService>(
            new Mock<ILogger<ProducerService>>().Object,
            mockKafkaConfig
        );

        _authService = new Service.Implementation.AuthService(
            _repositoryManagerMock.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _mapperManagerMock.Object,
            _mockProducerService.Object
        );

        _mockProducerService.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<object>()));
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsEmptyString()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            Password = "Password123!",
            Role = "Guest",
            Address = new()
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber",
            },
        };
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            UserName = "test@example.com",
        };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mapperManagerMock
            .Setup(m => m.ApplicationUserToUserDtoMapper.Map(It.IsAny<ApplicationUser>()))
            .Returns(new UserDto());

        Assert.DoesNotThrowAsync(async () => await _authService.Register(registrationRequestDto));
    }

    [Test]
    public async Task Register_UserCreationFails_ReturnsErrorDescription()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            Password = "Password123!",
            Role = "User",
            Address = new()
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber",
            },
        };

        var user = new ApplicationUser
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            UserName = "test@example.com",
        };

        var identityError = new IdentityError { Description = "Error creating user." };
        var identityResult = IdentityResult.Failed(identityError);

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _authService.Register(registrationRequestDto)
        );
        Assert.That(ex.Message, Is.EqualTo(identityError.Description));
    }

    [Test]
    public async Task Register_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            Password = "Password123!",
            Role = "User",
            Address = new()
            {
                City = "City",
                Country = "Country",
                PostNumber = "PostNumber",
                StreetName = "StreetName",
                StreetNumber = "StreetNumber",
            },
        };
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            FirstName = "First Name",
            LastName = "Last Name",
            UserName = "test@example.com",
        };

        _mapperManagerMock
            .Setup(m =>
                m.RegistrationsRequestDtoToApplicationUserMapper.Map(
                    It.IsAny<RegistrationRequestDto>()
                )
            )
            .Returns(user);

        _mockUserManager
            .Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Throws(new Exception("An error occurred during user creation"));

        var ex = Assert.ThrowsAsync<Exception>(
            async () => await _authService.Register(registrationRequestDto)
        );
        Assert.That(ex.Message, Is.EqualTo("An error occurred during user creation"));
    }

    [Test]
    public async Task Login_UserValid_ReturnsLoginResponseDto()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Username = "testuser@example.com",
            Password = "Password123!",
        };

        var user = new ApplicationUser
        {
            UserName = loginRequestDto.Username,
            Email = loginRequestDto.Username,
            NormalizedEmail = loginRequestDto.Username.ToUpper(),
            FirstName = "First Name",
            LastName = "Last Name",
            ExternalId = Guid.NewGuid(),
        };

        var userDto = new UserDto()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Id = user.ExternalId,
            Email = user.Email,
        };

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(user);

        _mapperManagerMock
            .Setup(m => m.ApplicationUserToUserDtoMapper.Map(It.IsAny<ApplicationUser>()))
            .Returns(userDto);

        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockUserManager
            .Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockJwtTokenGenerator
            .Setup(jwt => jwt.GenerateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .Returns("mock-token");

        // Act
        var result = await _authService.Login(loginRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.User.Email, Is.EqualTo(loginRequestDto.Username));
            Assert.That(result.Token, Is.EqualTo("mock-token"));
        });
    }

    [Test]
    public async Task Login_UserInvalid_ThrowsBadHttpRequestException()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Username = "testuser@example.com",
            Password = "Password123!",
        };

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        _mockUserManager
            .Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _authService.Login(loginRequestDto)
        );
        Assert.That(ex.Message, Is.EqualTo("Invalid username or password"));
    }
}
