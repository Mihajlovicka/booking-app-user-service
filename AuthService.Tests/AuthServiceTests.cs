using AuthService.Data;
using AuthService.Model;
using AuthService.Model.Dto;
using AuthService.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace AuthService.Tests;

public class Tests
{
    
    private Mock<AppDbContext> _mockDbContext;
    private Mock<UserManager<ApplicationUser>> _mockUserManager;
    private Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
    private IAuthService _authService;

 
    
    [SetUp]
    public void Setup()
    {


        _mockDbContext = new Mock<AppDbContext>();
        
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();

        _authService = new Service.AuthService(
            _mockDbContext.Object,
            _mockUserManager.Object,
            _mockJwtTokenGenerator.Object);
    }
    

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsEmptyString()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Name = "Test User",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            Role = "Guest"
        };
        var user = new ApplicationUser
        {
            Email = "test@example.com",
            Name = "Test User",
            PhoneNumber = "1234567890",
            UserName = "test@example.com"
        };
        var lst = Enumerable.AsEnumerable([user]);
        
        // Mock DbContext to return the mocked DbSet
        _mockDbContext.Setup(x => x.ApplicationUsers).ReturnsDbSet(lst);
        
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.Register(registrationRequestDto);

        // Assert
        Assert.AreEqual("", result);
    }


    [Test]
    public async Task Register_UserCreationFails_ReturnsErrorDescription()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Name = "Test User",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            Role = "User"
        };
    
        var identityError = new IdentityError { Description = "Error creating user." };
        var identityResult = IdentityResult.Failed(identityError);
    
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);
    
        // Act
        var result = await _authService.Register(registrationRequestDto);
    
        // Assert
        Assert.AreEqual(identityError.Description, result);
    }
    
    [Test]
    public async Task Register_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Name = "Test User",
            PhoneNumber = "1234567890",
            Password = "Password123!",
            Role = "User"
        };
    
        _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Throws(new System.Exception("Exception occurred"));
    
        // Act
        var result = await _authService.Register(registrationRequestDto);
    
        // Assert
        Assert.AreEqual("Error encountered!", result);
    }
    
    [Test]
    public async Task Login_UserValid_ReturnsLoginResponseDto()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Username = "testuser@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            UserName = loginRequestDto.Username,
            Email = loginRequestDto.Username,
            NormalizedEmail = loginRequestDto.Username.ToUpper(),
            Name = "Test User",
            PhoneNumber = "1234567890"
        };
        
        var lst = Enumerable.AsEnumerable([user]);
        
        // Mock DbContext to return the mocked DbSet
        _mockDbContext.Setup(x => x.ApplicationUsers).ReturnsDbSet(lst);

        _mockUserManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockUserManager.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockJwtTokenGenerator.Setup(jwt => jwt.GenerateToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .Returns("mock-token");

        // Act
        var result = await _authService.Login(loginRequestDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.User);
        Assert.AreEqual("mock-token", result.Token);
        Assert.AreEqual(loginRequestDto.Username, result.User.Email);
    }

    [Test]
    public async Task Login_UserInvalid_ReturnsLoginResponseDtoWithEmptyToken()
    {
        // Arrange
        var loginRequestDto = new LoginRequestDto
        {
            Username = "testuser@example.com",
            Password = "Password123!"
        };

        var lst = Enumerable.Empty<ApplicationUser>();
        
        // Mock DbContext to return the mocked DbSet
        _mockDbContext.Setup(x => x.ApplicationUsers).ReturnsDbSet(lst);

        _mockUserManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authService.Login(loginRequestDto);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.User);
        Assert.AreEqual("", result.Token);
    }

}