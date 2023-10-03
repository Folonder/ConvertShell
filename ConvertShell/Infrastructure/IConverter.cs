namespace ConvertShell.Infrastructure;

public interface IConverter
{
    public Task<byte[]> ConvertAsync(byte[] fileName, byte[] fileData);
}