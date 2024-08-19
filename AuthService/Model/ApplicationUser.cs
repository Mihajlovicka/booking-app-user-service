using Microsoft.AspNetCore.Identity;

namespace AuthService.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
