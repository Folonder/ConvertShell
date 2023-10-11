using ConvertShell.Infrastructure;

namespace ConvertShell.Services;

public class ConvertService : IConvertService
{
    private readonly IConverter _converter;

    public ConvertService(IConverter converter)
    {
        _converter = converter;
    }

    public async Task<byte[]> ConvertFile(string fileName, byte[] fileData, string outFileExtension)
    {
        return await _converter.ConvertAsync(fileName, fileData, outFileExtension);
    }
}