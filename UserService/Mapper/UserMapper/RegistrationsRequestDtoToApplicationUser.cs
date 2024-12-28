using UserService.Model.Dto;
using UserService.Model.Entity;

namespace UserService.Mapper.UserMapper;

public class RegistrationsRequestDtoToApplicationUser
    : BaseMapper<RegistrationRequestDto, ApplicationUser>
{
    public override ApplicationUser Map(RegistrationRequestDto source)
    {
        return new()
        {
            UserName = source.Email,
            Email = source.Email,
            NormalizedEmail = source.Email.ToUpper(),
            FirstName = source.FirstName,
            LastName = source.LastName,
            ExternalId = Guid.NewGuid(),
            Address = new()
            {
                City = source.Address.City,
                Country = source.Address.Country,
                PostNumber = source.Address.PostNumber,
                StreetName = source.Address.StreetName,
                StreetNumber = source.Address.StreetNumber,
            },
        };
    }
}
