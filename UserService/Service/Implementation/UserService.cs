using Microsoft.AspNetCore.Identity;
using UserService.Mapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Service.Contract;

namespace UserService.Service.Implementation;

public class UserService(
    IUserContextService userContextService,
    IMapperManager mapperManager,
    IRepositoryManager repositoryManager
) : IUserService
{
    public async Task<UserDto> GetLoggedUser()
    {
        var logged = await userContextService.GetCurrentUserAsync();
        logged = await repositoryManager.UserRepository.GetByUsername(logged.UserName);
        return mapperManager.ApplicationUserToUserDtoMapper.Map(logged);
    }

    public async Task<UserDto> Update(UserDto userDto)
    {
        ApplicationUser? user = await repositoryManager.UserRepository.GetByExternalId(userDto.Id);
        if (user == null)
            throw new BadHttpRequestException("User not found");
        if (
            await repositoryManager.UserRepository.UserNameExists(userDto.Id, userDto.Username)
            != null
        )
            throw new BadHttpRequestException("Username already exists");
        if (await repositoryManager.UserRepository.EmailExists(userDto.Id, userDto.Email) != null)
            throw new BadHttpRequestException("Email already exists");

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.UserName = userDto.Username;
        if (userDto.Address != null)
        {
            user.Address = new Address
            {
                City = userDto.Address.City,
                Country = userDto.Address.Country,
                PostNumber = userDto.Address.PostNumber,
                StreetName = userDto.Address.StreetName,
                StreetNumber = userDto.Address.StreetNumber,
            };
        }

        await repositoryManager.UserRepository.UpdateAsync(user);
        return mapperManager.ApplicationUserToUserDtoMapper.Map(user);
    }
}
