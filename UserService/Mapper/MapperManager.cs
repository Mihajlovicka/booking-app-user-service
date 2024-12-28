using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Mapper;

public class MapperManager(
    IBaseMapper<ApplicationUser, UserDto> applicationUserToUserDtoMapper,
    IBaseMapper<
        RegistrationRequestDto,
        ApplicationUser
    > registrationsRequestDtoToApplicationUserMapper
) : IMapperManager
{
    public IBaseMapper<ApplicationUser, UserDto> ApplicationUserToUserDtoMapper { get; } =
        applicationUserToUserDtoMapper;
    public IBaseMapper<
        RegistrationRequestDto,
        ApplicationUser
    > RegistrationsRequestDtoToApplicationUserMapper { get; } =
        registrationsRequestDtoToApplicationUserMapper;
}
