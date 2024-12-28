using UserService.Model.Dto;

namespace UserService.Service.Contract;

public interface IAuthService
{
    Task Register(RegistrationRequestDto registrationRequestDto);
    Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
}
