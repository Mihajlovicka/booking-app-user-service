using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Mapper;

public interface IMapperManager
{
    IBaseMapper<ApplicationUser, UserDto> ApplicationUserToUserDtoMapper { get; }
    IBaseMapper<
        RegistrationRequestDto,
        ApplicationUser
    > RegistrationsRequestDtoToApplicationUserMapper { get; }
}
