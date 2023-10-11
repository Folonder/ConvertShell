using System.Text;

namespace ConvertShell;

public static class Utils
{
    private static Random random = new Random();
    
    public static byte[] EncryptMd5(byte[] content)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            return md5.ComputeHash(content);
        }
    }

    public static byte[] StringToBytes(string content)
    {
        return Encoding.UTF8.GetBytes(content);
    }

    public static string BytesToString(byte[] content)
    {
        return Encoding.UTF8.GetString(content);
    }
    
    public static string getRandomHexString(int length)
    {
        const string chars = "abcdef0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}