namespace ConvertShell.Services
{
    public interface IConvertService
    {
        public Task<byte[]> ConvertFile(string fileName, byte[] fileData, string outFileExtension);
    }
}
