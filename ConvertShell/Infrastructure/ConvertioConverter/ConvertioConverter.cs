using System.Net;
using System.Text.Json;
using ConvertShell.Exceptions;
using Microsoft.Extensions.Options;
using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class ConvertioConverter : IConverter
{
    private readonly string _uploadMetaDataUrl;
    private readonly string _getFileUrl;
    private readonly ConvertioClient _client;
    private readonly ConvertioContentBuilder _contentBuilder;
    
 
    public ConvertioConverter(IOptions<ConvertioConverterOptions> options, ConvertioClient client, ConvertioContentBuilder convertioContentBuilder)
    {
        _uploadMetaDataUrl = options.Value.UploadMetaDataUrl;
        _getFileUrl = options.Value.GetFileUrl;
        _client = client;
        _contentBuilder = convertioContentBuilder;
    }

    public async Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension)
    {
        //Upload meta data and get url to upload file
        var metaData = _contentBuilder.MetaData(fileName, fileData, outFileExtension);
        
        var uploadMetaDataContent = _contentBuilder.UploadMetaDataContent(metaData);
        var uploadFileUrl = await _client.UploadMetaDataAsync(_uploadMetaDataUrl, uploadMetaDataContent);
        
        //Upload file
        var uploadFileContent = _contentBuilder.UploadFileContent(metaData);
        await _client.UploadFileAsync(uploadFileUrl, uploadFileContent);
        
        //Get url to download file
        var getDownloadUrlContent = _contentBuilder.GetDownloadUrlContent(metaData);
        var downloadUrl = await _client.GetDownloadUrl(_getFileUrl, getDownloadUrlContent, metaData);
        
        //Download url
        return await _client.DownloadFileAsync(downloadUrl);
    }
}