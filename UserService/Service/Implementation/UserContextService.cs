using Microsoft.AspNetCore.Identity;
using UserService.Model.Entity;
using UserService.Service.Contract;

namespace UserService.Service.Implementation;

public class UserContextService(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor
) : IUserContextService
{
    public async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        return user;
    }
}
