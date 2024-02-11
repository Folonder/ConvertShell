using System.Text;

namespace ConvertShell.Infrastructure;

public static class ConvertioContentUtils
{
    public static byte[] EncryptMd5(byte[] content)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return md5.ComputeHash(content);
    }

    public static byte[] StringToBytes(string content)
    {
        return Encoding.UTF8.GetBytes(content);
    }

    public static string BytesToString(byte[] content)
    {
        return Encoding.UTF8.GetString(content);
    }
}