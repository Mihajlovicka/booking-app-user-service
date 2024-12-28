using UserService.Model.Entity;

namespace UserService.Service.Contract;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
}
