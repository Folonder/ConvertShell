namespace ConvertShell.Extensions;

public static class FormFileExtensions
{
    public static async Task<byte[]> ReadFile(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}