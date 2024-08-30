using System.ComponentModel.DataAnnotations;
using AuthService.Model.Entity;

namespace AuthService.Model.Dto
{
    public class RegistrationRequestDto
    {
        public string Email { get; set; }

        public string Password { get; set; }

        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public string Role { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string LastName { get; set; }

        public AddressDto Address { get; set; }
    }
}