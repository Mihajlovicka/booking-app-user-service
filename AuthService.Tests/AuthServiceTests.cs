using AuthService.Data;
using AuthService.Mapper;
using AuthService.Model.Dto;
using AuthService.Model.Entity;
using AuthService.Repository.Contract;
using AuthService.Service.Contract;
using AuthService.Service.MessagingService;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Confluent.Kafka;


namespace AuthService.Tests;

[TestFixture]
[Category("Unit")]
public class Tests
{
    private Mock<AppDbContext> _mockDbContext;
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<ApplicationUser>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    // private Mock<ProducerService> _mockProducerService;
    private IAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockDbContext = new Mock<AppDbContext>();

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

        _authService = new Service.Implementation.AuthService(
            _repositoryManagerMock.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _mapperManagerMock.Object,
            // _mockProducerService.Object
        );
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

        // Act
        var result = await _authService.Register(registrationRequestDto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ErrorMessage, Is.EqualTo(""));
        });
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

        // Act
        var result = await _authService.Register(registrationRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo(identityError.Description));
        });
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
            .Throws(new System.Exception("An error occurred during user creation"));

        // Act
        var result = await _authService.Register(registrationRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("An error occurred during user creation"));
        });
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
            Assert.That(result.Success, Is.True);
            Assert.That(
                (result.Result as LoginResponseDto).User.Email,
                Is.EqualTo(loginRequestDto.Username)
            );
            Assert.That((result.Result as LoginResponseDto).Token, Is.EqualTo("mock-token"));
        });
    }

    [Test]
    public async Task Login_UserInvalid_ReturnsLoginResponseDtoWithEmptyToken()
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

        // Act
        var result = await _authService.Login(loginRequestDto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Result, Is.Null);
        });
    }
}
