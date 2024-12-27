using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Mapper.UserMapper;

public class ApplicationUserToUserDto : BaseMapper<ApplicationUser, UserDto>
{
    public override UserDto Map(ApplicationUser source)
    {
        return new()
        {
            Email = source.Email!,
            Id = source.ExternalId,
            FirstName = source.FirstName,
            LastName = source.LastName,
        };
    }
}
