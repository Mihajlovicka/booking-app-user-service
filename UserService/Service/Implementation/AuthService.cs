using Microsoft.AspNetCore.Identity;
using UserService.Mapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Service.Contract;
using UserService.Service.MessagingService;

namespace UserService.Service.Implementation;

public class AuthService(
    IRepositoryManager _repository,
    UserManager<ApplicationUser> _userManager,
    IJwtTokenGenerator _jwtTokenGenerator,
    IMapperManager _mapperManager,
    ProducerService producerService
) : IAuthService
{
    public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
    {
        var user = await _repository.UserRepository.GetByUsername(loginRequestDto.Username);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
        {
            throw new BadHttpRequestException("Invalid username or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        var userDto = _mapperManager.ApplicationUserToUserDtoMapper.Map(user);
        userDto.Role = roles.FirstOrDefault();

        return new LoginResponseDto { User = userDto, Token = token };
    }

    public async Task Register(RegistrationRequestDto registrationRequestDto)
    {
        var user = _mapperManager.RegistrationsRequestDtoToApplicationUserMapper.Map(
            registrationRequestDto
        );
        var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
        if (!result.Succeeded)
            throw new BadHttpRequestException(
                result.Errors.FirstOrDefault()?.Description ?? "Registration failed"
            );

        var createdUser = await _repository.UserRepository.GetByUsername(
            registrationRequestDto.Email
        );
        if (createdUser == null)
            throw new BadHttpRequestException("User creation failed");

        await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
        var userDto = _mapperManager.ApplicationUserToUserDtoMapper.Map(createdUser);
        userDto.Role = registrationRequestDto.Role;
        _ = producerService.ProduceAsync(KafkaTopic.UserCreated.ToString(), userDto);
    }
}
