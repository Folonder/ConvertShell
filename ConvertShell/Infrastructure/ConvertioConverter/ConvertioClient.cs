using System.Net;
using System.Text.Json;
using ConvertShell.Exceptions;

 
namespace ConvertShell.Infrastructure;
 
public class ConvertioClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConvertioClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
 
    public async Task<string> UploadMetaDataAsync(string url, MultipartFormDataContent content)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent is "Malformed request" or "No minutes left")
        {
            throw new WebException(responseContent);
        }
        return responseContent;
    }
 
    public async Task UploadFileAsync(string url, MultipartFormDataContent content)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (responseContent != "OK")
        {
            throw new WebException(responseContent);
        }
    }
 
    public async Task<string> GetDownloadUrl(string url, FormUrlEncodedContent content, MetaData metaData)
    {
        var httpClient = _httpClientFactory.CreateClient();
 
        var retryingHttpClient = new RetryingClient(new ConvertRetryParams(metaData.FileData.Length, metaData.OutFileExtension));
 
        return await retryingHttpClient.ExecuteWithRetryIfResultEmpty(async () => await TryGetDownloadUrl(url, content, metaData, httpClient));
        
    }

    private async Task<string> TryGetDownloadUrl(string url, FormUrlEncodedContent content, MetaData metaData, HttpClient httpClient)
    {
        var response = await httpClient.PostAsync(url, content);
 
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
        throw new DownloadUrlException($"Can not get download url");
    }
 
    public async Task<byte[]> DownloadFileAsync(string url)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync();
    }
}