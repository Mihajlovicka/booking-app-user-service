using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Dto;

public class AddressDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string StreetNumber { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string StreetName { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string City { get; set; }

    public string PostNumber { get; set; }
    public string Country { get; set; }
}
