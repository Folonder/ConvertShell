using ConvertShell.Infrastructure;
using static ConvertShell.Utils;

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
        ValidateFile(file, new string[] {".txt"});
        byte[] fileData = await ReadFile(file);
        return await _converter.ConvertAsync(StringToBytes(file.FileName), fileData);
    }

    private static void ValidateFile(IFormFile? file, string[] fileExtensions)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded.");
        }

        string fileExtension = Path.GetExtension(file.FileName).ToLower();

        bool anyMatch = false;
        
        foreach (var ext in fileExtensions)
        {
            if (ext == fileExtension)
            {
                anyMatch = true;
                break;
            }
        }
        
        if (!anyMatch)
        {
            throw new ArgumentException("Unsupported file type. Only TXT files are allowed.");
        }
    }

    private static async Task<byte[]> ReadFile(IFormFile? file)
    {
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}



