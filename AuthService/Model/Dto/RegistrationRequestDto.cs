using System.ComponentModel.DataAnnotations;

namespace AuthService.Model.Dto
{
    public class RegistrationRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        [EnumDataType(typeof(Role), ErrorMessage = "Invalid role.")]
        public string Role { get; set; }
    }
}
