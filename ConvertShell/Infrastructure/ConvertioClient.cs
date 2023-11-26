using System.Net;
using System.Text.Json;
using ConvertShell.Exceptions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using static ConvertShell.Utils;
 
namespace ConvertShell.Infrastructure;
 
public class ConvertioClient : IConverter
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    private readonly string _uploadMetaDataUrl;
    private readonly string _getFileUrl;
    private readonly MetaData _metadata;
 
    public ConvertioClient(IOptions<ConvertioClientOptions> options, IHttpClientFactory httpClientFactory, MetaData metadata)
    {
        var _options = options.Value;
        _uploadMetaDataUrl = _options.UploadMetaDataUrl;
        _getFileUrl = _options.GetFileUrl;
        _httpClientFactory = httpClientFactory;
        _metadata = metadata;
    }
 
    public async Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension)
    {
        var metaData = _metadata.Create(fileName, fileData, outFileExtension);
        var uploadFileUrl = await UploadMetadataAsync(metaData);

        await UploadFileAsync(uploadFileUrl, metaData);

        var downloadUrl = await GetDownloadUrl(metaData);
 
        return await DownloadFileAsync(downloadUrl);
    }
 
    private async Task<string> UploadMetadataAsync(MetaData metaData)
    {
        var formContent = CreateContent(metaData);
 
        var httpClient = _httpClientFactory.CreateClient();
        
        var response = await httpClient.PostAsync(_uploadMetaDataUrl, formContent);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent is "Malformed request" or "No minutes left")
        {
            throw new WebException(responseContent);
        }
 
        return responseContent;
    }

    private MultipartFormDataContent CreateContent(MetaData metaData)
    {
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(metaData.FileId), "file_id");
        formContent.Add(new StringContent(metaData.SessionId), "session_id");
        formContent.Add(new StringContent(metaData.FileName), "user_fn");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(StringToBytes(metaData.FileName)))),
            "user_fn_hash");
        formContent.Add(new StringContent($"{metaData.FileData.Length}"), "file_size");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(metaData.FileData))), "file_hash");
        formContent.Add(new StringContent(metaData.OutFileExtension), "file_out_format");
        formContent.Add(new StringContent(metaData.PackId), "pack_id");
        return formContent;
    }
 
    private async Task UploadFileAsync(string url, MetaData metaData)
    {
        var fileContent = new ByteArrayContent(metaData.FileData);
        fileContent.Headers.Add("Content-Type", "text/plain");
 
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(metaData.FileId), "file_id");
        content.Add(fileContent, "file", metaData.FileName);
 
        var httpClient = _httpClientFactory.CreateClient();
 
        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent != "OK")
        {
            throw new WebException(responseContent);
        }
    }
 
    private async Task<string> GetDownloadUrl(MetaData metaData)
    {
        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id", metaData.FileId),
            new KeyValuePair<string, string>("type", "convert")
        });
 
        var httpClient = _httpClientFactory.CreateClient();
 
        var retryingHttpClient = new RetryingHttpClient(new ConvertRetryParams(metaData.FileData.Length, metaData.OutFileExtension));
 
        return await retryingHttpClient.ExecuteWithRetryIfResultEmpty(async () => await TryGetDownloadUrl(metaData, data, httpClient));
        
    }

    private async Task<string> TryGetDownloadUrl(MetaData metaData, FormUrlEncodedContent data, HttpClient httpClient)
    {
        var response = await httpClient.PostAsync(_getFileUrl, data);
 
        if (response.IsSuccessStatusCode)
        {
            var dict =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(
                    await response.Content.ReadAsStringAsync());
            var downloadUrl = dict["convert"][metaData.FileId]["out_url"].ToString();
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                return downloadUrl;
            }
        }
        throw new DownloadUrlException($"Can't get download url");
    }
 
    private async Task<byte[]> DownloadFileAsync(string url)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync();
    }
}