using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Mapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Service.Contract;
using UserService.Service.MessagingService;

namespace UserService.Tests;

[TestFixture]
[Category("Unit")]
public class ChangePassTests
{
    private Mock<IRepositoryManager> _repositoryManagerMock;
    private Mock<IMapperManager> _mapperManagerMock;
    private Mock<UserManager<ApplicationUser>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private Mock<IUserContextService> _mockUserContextService;
    private Mock<ProducerService> _mockProducerService;
    private IAuthService _authService;

    [SetUp]
    public void Setup()
    {
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _mapperManagerMock = new Mock<IMapperManager>();
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
        var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
        var mockKafkaConfig = Mock.Of<IOptions<ProducerConfig>>(options =>
            options.Value == producerConfig
        );
        _mockProducerService = new Mock<ProducerService>(
            new Mock<ILogger<ProducerService>>().Object,
            mockKafkaConfig
        );
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mockUserContextService = new Mock<IUserContextService>();

        _repositoryManagerMock
            .Setup(repo => repo.UserRepository)
            .Returns(new Mock<IUserRepository>().Object);

        _authService = new Service.Implementation.AuthService(
            _repositoryManagerMock.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object,
            _mapperManagerMock.Object,
            _mockProducerService.Object,
            _mockUserContextService.Object
        );
    }

    [Test]
    public async Task ChangePassword_UserNotFound_ThrowsBadHttpRequestException()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        _mockUserContextService
            .Setup(ucs => ucs.GetCurrentUserAsync())
            .ReturnsAsync(new ApplicationUser { UserName = "testuser@example.com" });

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _authService.ChangePassword(changePasswordDto)
        );
        Assert.That(ex.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task ChangePassword_PasswordsDoNotMatch_ThrowsBadHttpRequestException()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!",
        };

        _mockUserContextService
            .Setup(ucs => ucs.GetCurrentUserAsync())
            .ReturnsAsync(new ApplicationUser { UserName = "testuser@example.com" });

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { UserName = "testuser@example.com" });

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _authService.ChangePassword(changePasswordDto)
        );
        Assert.That(ex.Message, Is.EqualTo("Passwords do not match"));
    }

    [Test]
    public async Task ChangePassword_PasswordChangeFails_ThrowsBadHttpRequestException()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        var user = new ApplicationUser { UserName = "testuser@example.com" };

        _mockUserContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(user);

        var identityError = new IdentityError
        {
            Description = "Password change failed. Old password is incorrect.",
        };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager
            .Setup(um =>
                um.ChangePasswordAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(identityResult);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _authService.ChangePassword(changePasswordDto)
        );
        Assert.That(ex.Message, Is.EqualTo(identityError.Description));
    }

    [Test]
    public async Task ChangePassword_PasswordChangedSuccessfully_DoesNotThrow()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
        };

        var user = new ApplicationUser { UserName = "testuser@example.com" };

        _mockUserContextService.Setup(ucs => ucs.GetCurrentUserAsync()).ReturnsAsync(user);

        _repositoryManagerMock
            .Setup(r => r.UserRepository.GetByUsername(It.IsAny<string>()))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(um =>
                um.ChangePasswordAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(IdentityResult.Success);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _authService.ChangePassword(changePasswordDto));
    }
}
