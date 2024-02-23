using System.Text;

namespace ConvertShell.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] EncryptMd5(this byte[] content)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return md5.ComputeHash(content);
    }



    public static string BytesToString(this byte[] content)
    {
        return Encoding.UTF8.GetString(content);
    }
}