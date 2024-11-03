using AuthService.Mapper;
using AuthService.Model.Dto;
using AuthService.Model.Entity;
using AuthService.Model.ServiceResponse;
using AuthService.Repository.Contract;
using AuthService.Service.Contract;
using AuthService.Service.MessagingService;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Service.Implementation;

public class AuthService(
    IRepositoryManager _repository,
    UserManager<ApplicationUser> _userManager,
    IJwtTokenGenerator _jwtTokenGenerator,
    IMapperManager _mapperManager,
    ProducerService _kafkaProducer
) : IAuthService
{
    public async Task<ResponseBase> Login(LoginRequestDto loginRequestDto)
    {
        //_kafkaProducer.ProduceAsync(KafkaTopic.TestTopic.ToString(), loginRequestDto);
        var response = new ResponseBase();
        var user = await _repository.UserRepository.GetByUsername(loginRequestDto.Username);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
        {
            return CreateErrorResponse("Username or password is incorrect");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        response.Result = CreateLoginResponse(user, token, roles);
        return response;
    }

    public async Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto)
    {
        var response = new ResponseBase();
        var user = _mapperManager.RegistrationsRequestDtoToApplicationUserMapper.Map(
            registrationRequestDto
        );
        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
            if (!result.Succeeded)
            {
                return CreateErrorResponse(
                    result.Errors.FirstOrDefault()?.Description ?? "Registration failed"
                );
            }
        }
        catch (Exception ex)
        {
            return CreateErrorResponse("An error occurred during user creation");
        }

        var createdUser = await _repository.UserRepository.GetByUsername(
            registrationRequestDto.Email
        );
        if (createdUser == null)
        {
            return CreateErrorResponse("User creation failed");
        }

        await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
        return response;
    }

    private ResponseBase CreateErrorResponse(string errorMessage)
    {
        return new ResponseBase { Success = false, ErrorMessage = errorMessage };
    }

    private LoginResponseDto CreateLoginResponse(
        ApplicationUser user,
        string token,
        IList<string> roles
    )
    {
        var userDto = _mapperManager.ApplicationUserToUserDtoMapper.Map(user);
        userDto.Role = roles.FirstOrDefault();

        return new LoginResponseDto { User = userDto, Token = token };
    }
}
