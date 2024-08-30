using AuthService.Model;
using AuthService.Model.Dto;
using AuthService.Model.Entity;

namespace AuthService.Mapper;

public class MapperManager(
    IBaseMapper<ApplicationUser, UserDto> applicationUserToUserDtoMapper,
    IBaseMapper<RegistrationRequestDto, ApplicationUser> registrationsRequestDtoToApplicationUserMapper) : IMapperManager
{
    public IBaseMapper<ApplicationUser, UserDto> ApplicationUserToUserDtoMapper { get; } = applicationUserToUserDtoMapper;
    public IBaseMapper<RegistrationRequestDto, ApplicationUser> RegistrationsRequestDtoToApplicationUserMapper { get; } = registrationsRequestDtoToApplicationUserMapper;
}
