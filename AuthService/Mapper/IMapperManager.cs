using AuthService.Model;
using AuthService.Model.Dto;
using AuthService.Model.Entity;

namespace AuthService.Mapper;

public interface IMapperManager
{
    IBaseMapper<ApplicationUser, UserDto> ApplicationUserToUserDtoMapper { get; }
    IBaseMapper<RegistrationRequestDto, ApplicationUser> RegistrationsRequestDtoToApplicationUserMapper { get; }
}