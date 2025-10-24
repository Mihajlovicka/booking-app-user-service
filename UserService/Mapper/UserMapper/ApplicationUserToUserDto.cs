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
            Username = source.UserName,
            Address = source.Address != null ? new()
            {
                City = source.Address.City,
                Country = source.Address.Country,
                PostNumber = source.Address.PostNumber,
                StreetName = source.Address.StreetName,
                StreetNumber = source.Address.StreetNumber,
            } : null
        };
    }
}
