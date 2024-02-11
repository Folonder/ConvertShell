namespace ConvertShell.Controllers;

public static class FileUtils
{
    public static async Task<byte[]> ReadFile(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
    
    public static string ChangeFileExtension(string fileName, string newExtension)
    {
        var lastDotIndex = fileName.LastIndexOf('.');

        if (lastDotIndex < 0) return $"{fileName}.{newExtension}";
        
        var nameWithoutExtension = fileName.Substring(0, lastDotIndex);
        return $"{nameWithoutExtension}.{newExtension}";
    }
}