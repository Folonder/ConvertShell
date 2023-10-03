using ConvertShell.Infrastructure;

namespace ConvertShell.Services;

public class ConvertService : IConvertService
{
    private readonly IConverter _converter;

    public ConvertService(IConverter converter)
    {
        _converter = converter;
    }

    public async Task<byte[]> ToPdf(IFormFile? file)
    {
        ValidateFile(file, new string[] { ".txt" });
        var fileData = await ReadFile(file);
        return await _converter.ConvertAsync(file.FileName, fileData, "PDF");
    }

    private static void ValidateFile(IFormFile? file, string[] fileExtensions)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded.");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        var anyMatch = fileExtensions.Any(ext => ext == fileExtension);

        if (!anyMatch)
        {
            throw new ArgumentException(
                $"Unsupported file type. Only {string.Join(", ", fileExtensions)} files are allowed.");
        }
    }

    private static async Task<byte[]> ReadFile(IFormFile? file)
    {
        using var memoryStream = new MemoryStream();
        
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}