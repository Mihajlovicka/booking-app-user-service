using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Model.Entity;

[Table("addresses")]
public class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; } //= Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("street_number")]
    public string StreetNumber { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("street_name")]
    public string StreetName { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("city")]
    public string City { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("post_number")]
    public string PostNumber { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("country")]
    public string Country { get; set; }
}
