using UserService.Model.Dto;

namespace UserService.Service.Contract;

public interface IUserService
{
    Task<UserDto> GetLoggedUser();
    Task<UserDto> Update(UserDto userDto);
}
