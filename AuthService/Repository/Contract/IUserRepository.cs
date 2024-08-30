using AuthService.Model;
using AuthService.Model.Entity;

namespace AuthService.Repository.Contract;

public interface IUserRepository : ICrudRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByExternalId(Guid externalId);
    Task<ApplicationUser> GetByUsername(string username);
}