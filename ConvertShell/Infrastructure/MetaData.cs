using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class MetaData
{
    public string FileId {get;}
    public string PackId {get;}
    public string SessionId {get;}
    public string FileName {get;}
    public byte[] FileData {get;}
    public string OutFileExtension {get;}

    public MetaData(string fileName, byte[] fileData, string outFileExtension)
    { 
        FileId = getRandomHexString(32);
        PackId = getRandomHexString(6);
        SessionId = getRandomHexString(32);

        FileName = fileName;
        FileData = fileData;
        OutFileExtension = outFileExtension;
    }
}