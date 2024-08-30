using AuthService.Model;
using AuthService.Model.Entity;

namespace AuthService.Service.Contract
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
