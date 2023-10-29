namespace ConvertShell.Infrastructure;

public interface IConverter
{
    public Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension);
}