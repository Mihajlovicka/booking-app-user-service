using AuthService.Model;
using AuthService.Model.Dto;
using AuthService.Model.ServiceResponse;
using Org.BouncyCastle.Bcpg;

namespace AuthService.Service.Contract
{
    public interface IAuthService
    {
        Task<ResponseBase> Register(RegistrationRequestDto registrationRequestDto);
        Task<ResponseBase> Login(LoginRequestDto loginRequestDto);
    }
}
