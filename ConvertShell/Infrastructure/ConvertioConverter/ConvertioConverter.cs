using Microsoft.Extensions.Options;


namespace ConvertShell.Infrastructure;

public class ConvertioConverter : IConverter
{
    private readonly string _uploadMetaDataUrl;
    private readonly string _getFileUrl;
    private readonly ConvertioClient _client;
    private readonly ConvertioContent _content;
    
 
    public ConvertioConverter(IOptions<ConvertioConverterOptions> options, ConvertioClient client, ConvertioContent convertioContent)
    {
        _uploadMetaDataUrl = options.Value.UploadMetaDataUrl;
        _getFileUrl = options.Value.GetFileUrl;
        _client = client;
        _content = convertioContent;
    }

    public async Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension)
    {
        //Upload meta data and get url to upload file
        var metaData = _content.MetaData(fileName, fileData, outFileExtension);
        
        var uploadMetaDataContent = _content.UploadMetaDataContent(metaData);
        var uploadFileUrl = await _client.UploadMetaDataAsync(_uploadMetaDataUrl, uploadMetaDataContent);
        
        //Upload file
        var uploadFileContent = _content.UploadFileContent(metaData);
        await _client.UploadFileAsync(uploadFileUrl, uploadFileContent);
        
        //Get url to download file
        var getDownloadUrlContent = _content.GetDownloadUrlContent(metaData);
        var downloadUrl = await _client.GetDownloadUrl(_getFileUrl, getDownloadUrlContent, metaData);
        
        //Download url
        return await _client.DownloadFileAsync(downloadUrl);
    }
}