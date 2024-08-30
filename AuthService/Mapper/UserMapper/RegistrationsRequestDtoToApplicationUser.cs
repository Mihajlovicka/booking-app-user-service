using AuthService.Model;
using AuthService.Model.Dto;
using AuthService.Model.Entity;

namespace AuthService.Mapper.UserMapper;

public class RegistrationsRequestDtoToApplicationUser: BaseMapper<RegistrationRequestDto, ApplicationUser>
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
                StreetNumber = source.Address.StreetNumber
            }
        };
    }
}