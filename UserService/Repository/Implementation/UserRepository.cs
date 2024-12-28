using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Model.Entity;
using UserService.Repository.Contract;

namespace UserService.Repository.Implementation;

public class UserRepository(AppDbContext context)
    : CrudRepository<ApplicationUser>(context),
        IUserRepository
{
    public async Task<ApplicationUser?> GetByExternalId(Guid externalId) =>
        await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.ExternalId == externalId);

    public async Task<ApplicationUser> GetByUsername(string username) =>
        await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == username);
}
