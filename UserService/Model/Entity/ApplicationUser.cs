using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace UserService.Model.Entity;

public class ApplicationUser : IdentityUser<int>
{
    [Column("external_id")] 
    public Guid ExternalId { get; set; }

    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; }

    [MaxLength(100)]
    [Required]
    [Column("last_name")]
    public string LastName { get; set; }

    [Column("address_id")]
    [ForeignKey("Address")]
    public int AddressId { get; set; }

    public Address Address { get; set; }
}
