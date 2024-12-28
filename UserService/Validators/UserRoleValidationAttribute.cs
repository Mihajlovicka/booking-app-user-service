using System.ComponentModel.DataAnnotations;
using UserService.Model.Entity;

namespace UserService.Validators;

public class UserRoleValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || !Enum.IsDefined(typeof(Role), value))
        {
            return new ValidationResult("Invalid user role.");
        }

        return ValidationResult.Success!;
    }
}
