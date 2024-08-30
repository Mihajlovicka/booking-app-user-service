using AuthService.Data;
using AuthService.Model;
using AuthService.Model.Entity;
using AuthService.Repository.Contract;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repository.Implementation;

public class UserRepository(AppDbContext context) : CrudRepository<ApplicationUser>(context), IUserRepository
{
    public async Task<ApplicationUser?> GetByExternalId(Guid externalId)
        => await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.ExternalId == externalId);
    
    public async Task<ApplicationUser> GetByUsername(string username)
        => await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.UserName == username);
}