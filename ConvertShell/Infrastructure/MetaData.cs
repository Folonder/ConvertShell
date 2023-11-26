using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class MetaData
{
    public string FileId {get; private set; }
    public string PackId {get; private set;}
    public string SessionId {get; private set;}
    public string FileName {get; private set;}
    public byte[] FileData {get; private set;}
    public string OutFileExtension {get; private set;}

    public virtual MetaData Create(string fileName, byte[] fileData, string outFileExtension)
    {
        var metaData  = new MetaData
        {
            FileId = getRandomHexString(32),
            PackId = getRandomHexString(6),
            SessionId = getRandomHexString(32),
            FileName = fileName,
            FileData = fileData,
            OutFileExtension = outFileExtension
        };
        return metaData;
    }
}