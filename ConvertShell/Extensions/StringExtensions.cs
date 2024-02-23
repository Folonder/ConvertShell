using System.Text;

namespace ConvertShell.Extensions;

public static class StringExtensions
{
    public static string ChangeFileExtension(this string fileName, string newExtension)
    {
        var lastDotIndex = fileName.LastIndexOf('.');

        if (lastDotIndex < 0) return $"{fileName}.{newExtension}";
        
        var nameWithoutExtension = fileName.Substring(0, lastDotIndex);
        return $"{nameWithoutExtension}.{newExtension}";
    }
    
    public static byte[] StringToBytes(this string content)
    {
        return Encoding.UTF8.GetBytes(content);
    }
}