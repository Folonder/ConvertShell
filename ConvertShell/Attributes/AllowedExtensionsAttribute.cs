using System.ComponentModel.DataAnnotations;

namespace ConvertShell.Attributes;

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;
    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension?.ToLower()))
            {
                return new ValidationResult(GetErrorMessage());
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("File can't be null");
    }

    public string GetErrorMessage()
    {
        return $"This file extension is not allowed!";
    }
}