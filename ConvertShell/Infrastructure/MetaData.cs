namespace ConvertShell.Infrastructure;

public class MetaData
{
    private static readonly Random Random = new Random();
    
    public string FileId {get; private set; }
    public string PackId {get; private set;}
    public string SessionId {get; private set;}
    public string FileName {get; private set;}
    public byte[] FileData {get; private set;}
    public string OutFileExtension {get; private set;}

    public static MetaData Create(string fileName, byte[] fileData, string outFileExtension)
    {
        var metaData  = new MetaData
        {
            FileId = GetRandomHexString(32),
            PackId = GetRandomHexString(6),
            SessionId = GetRandomHexString(32),
            FileName = fileName,
            FileData = fileData,
            OutFileExtension = outFileExtension
        };
        return metaData;
    }
    
    private static string GetRandomHexString(int length)
    {
        const string chars = "abcdef0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}