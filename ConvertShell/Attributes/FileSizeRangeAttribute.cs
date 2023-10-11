using System.ComponentModel.DataAnnotations;

namespace ConvertShell.Attributes;

public class FileSizeRangeAttribute : ValidationAttribute
{
    private readonly int _minFileSize;
    private readonly int _maxFileSize;
    public FileSizeRangeAttribute(int minFileSize, int maxFileSize)
    {
        _minFileSize = minFileSize;
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxFileSize || file.Length < _minFileSize)
            {
                return new ValidationResult(GetErrorMessage());
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("File can't be null");
    }

    public string GetErrorMessage()
    {
        return $"Allowed file size range is [{_minFileSize}, {_maxFileSize}] bytes.";
    }
}