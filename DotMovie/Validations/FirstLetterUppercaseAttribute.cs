using System.ComponentModel.DataAnnotations;

namespace DotMovie.Entities;

public class FirstLetterUppercaseAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var fLetter = value.ToString()[0].ToString();
        if (fLetter != fLetter.ToUpper())
        {
            return new ValidationResult("First letter should be uppercase");

        }

        return ValidationResult.Success;
    }
}