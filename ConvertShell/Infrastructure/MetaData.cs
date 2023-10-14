using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class MetaData
{
    public readonly string fileId;
    public readonly string packId;
    public readonly string sessionId;
    public readonly string fileName;
    public readonly byte[] fileData;
    public readonly string outFileExtension;

    public MetaData(string fileName, byte[] fileData, string outFileExtension)
    {
        fileId = getRandomHexString(32);
        packId = getRandomHexString(6);
        sessionId = getRandomHexString(32);

        this.fileName = fileName;
        this.fileData = fileData;
        this.outFileExtension = outFileExtension;
    }
}