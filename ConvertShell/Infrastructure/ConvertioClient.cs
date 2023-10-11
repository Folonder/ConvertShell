using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class ConvertioClient : IConverter
{
    private MetaData _metaData;
    private readonly string _uploadMetaDataUrl;
    private readonly string _getFileUrl;

    public ConvertioClient(IOptions<ConvertioClientOptions> options)
    {
        var _options = options.Value;
        _uploadMetaDataUrl = _options.UploadMetaDataUrl;
        _getFileUrl = _options.GetFileUrl;
    }

    public async Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension)
    {
        UpdateMetaData(fileName, fileData, outFileExtension);
        var uploadFileUrl = await UploadMetadataAsync();
        await UploadFileAsync(uploadFileUrl);

        var downloadUrl = String.Empty;
        
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(3000);
            downloadUrl = await TryGetUrl();
            
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                break;
            }
        }
        return await DownloadFileAsync(downloadUrl);
    }


    private void UpdateMetaData(string fileName, byte[] fileData, string outFileExtension)
    {
        _metaData = new MetaData(fileName, fileData, outFileExtension);
    }

    private async Task<string> UploadMetadataAsync()
    {
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(_metaData.fileId), "file_id");
        formContent.Add(new StringContent(_metaData.sessionId), "session_id");
        formContent.Add(new StringContent(_metaData.fileName), "user_fn");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(StringToBytes(_metaData.fileName)))),
            "user_fn_hash");
        formContent.Add(new StringContent($"{_metaData.fileData.Length}"), "file_size");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(_metaData.fileData))), "file_hash");
        formContent.Add(new StringContent(_metaData.outFileExtension), "file_out_format");
        formContent.Add(new StringContent(_metaData.packId), "pack_id");

        using var httpClient = new HttpClient();

        var response = await httpClient.PostAsync(_uploadMetaDataUrl, formContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent is "Malformed request" or "No minutes left")
        {
            throw new WebException(responseContent);
        }

        return responseContent;
    }

    private async Task UploadFileAsync(string url)
    {
        var fileContent = new ByteArrayContent(_metaData.fileData);
        fileContent.Headers.Add("Content-Type", "text/plain");

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(_metaData.fileId), "file_id");
        content.Add(fileContent, "file", _metaData.fileName);

        using var httpClient = new HttpClient();

        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent != "OK")
        {
            throw new WebException(responseContent);
        }
    }
    
    private async Task<string> TryGetUrl()
    {
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id", _metaData.fileId),
            new KeyValuePair<string, string>("type", "convert")
        });

        using var httpClient = new HttpClient();
        
        var response = await httpClient.PostAsync(_getFileUrl, data);
        var dict =
            JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(
                await response.Content.ReadAsStringAsync());
        return dict["convert"][_metaData.fileId]["out_url"].ToString();
    }

    private async Task<byte[]> DownloadFileAsync(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync();
    }
}


public struct MetaData
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