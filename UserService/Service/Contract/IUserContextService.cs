using UserService.Model.Entity;

namespace UserService.Service.Contract;

public interface IUserContextService
{
    Task<ApplicationUser> GetCurrentUserAsync();
}
