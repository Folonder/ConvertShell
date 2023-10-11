using ConvertShell.Infrastructure;

namespace ConvertShell.Services;

public class ConvertService : IConvertService
{
    private readonly IConverter _converter;

    public ConvertService(IConverter converter)
    {
        _converter = converter;
    }

    public async Task<byte[]> ToPdf(IFormFile file)
    {
        var fileData = await ReadFile(file);
        return await _converter.ConvertAsync(file.FileName, fileData, "PDF");
    }

    private static async Task<byte[]> ReadFile(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}