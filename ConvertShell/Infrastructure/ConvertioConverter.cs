using System.Net;
using System.Text.Json;
using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class ConvertioConverter : IConverter
{
    private MetaData _metaData;

    public async Task<byte[]> ConvertAsync(string fileName, byte[] fileData, string outFileExtension)
    {
        UpdateMetaData(fileName, fileData, outFileExtension);
        var uploadFileUrl = await UploadMetadataAsync();
        await UploadFileAsync(uploadFileUrl);

        string downloadUrl = "";
        
        for (int i = 0; i < 10 && string.IsNullOrEmpty(downloadUrl); i++)
        {
            await Task.Delay(3000);
            downloadUrl = await GetFileStatusAsync();
        }
        return await DownloadFileAsync(downloadUrl);
    }


    private void UpdateMetaData(string fileName, byte[] fileData, string outFileExtension)
    {
        _metaData = new MetaData(fileName, fileData, outFileExtension);
    }

    private async Task<string> UploadMetadataAsync()
    {
        const string url = "https://convertio.co/process/upload_metadata";

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

        var response = await httpClient.PostAsync(url, formContent);
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
    
    private async Task<string> GetFileStatusAsync()
    {
        const string url = "https://convertio.co/process/get_file_status";

        var data = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id", _metaData.fileId),
            new KeyValuePair<string, string>("type", "convert")
        });

        using var httpClient = new HttpClient();
        
        var response = await httpClient.PostAsync(url, data);
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
    public string fileId;
    public string packId;
    public string sessionId;
    public string fileName;
    public byte[] fileData;
    public string outFileExtension;

    public MetaData(string fileName, byte[] fileData, string outFileExtension)
    {
        fileId = RandomString(32);
        packId = RandomString(6);
        sessionId = RandomString(32);

        this.fileName = fileName;
        this.fileData = fileData;
        this.outFileExtension = outFileExtension;
    }
}