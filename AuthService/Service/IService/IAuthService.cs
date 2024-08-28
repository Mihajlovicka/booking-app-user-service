using AuthService.Model.Dto;
using Org.BouncyCastle.Bcpg;

namespace AuthService.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
    }
}
