using UserService.Model.Entity;

namespace UserService.Repository.Contract;

public interface IUserRepository : ICrudRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByExternalId(Guid externalId);
    Task<ApplicationUser> GetByUsername(string username);
    Task<ApplicationUser?> UserNameExists(Guid externalId, string username);
    Task<ApplicationUser?> EmailExists(Guid externalId, string email);
}
