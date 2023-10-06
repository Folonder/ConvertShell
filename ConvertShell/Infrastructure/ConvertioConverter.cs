using System.Net;
using System.Text.Json;
using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class ConvertioConverter : IConverter
{
    private string FileId { get; set; }
    private string PackId { get; set; }
    private string SessionId { get; set; }
    private string FileName { get; set; }
    private byte[] FileData { get; set; }
    private string OutFileExtension { get; set; }

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
        FileId = RandomString(32);
        PackId = RandomString(6);
        SessionId = RandomString(32);

        FileName = fileName;
        FileData = fileData;
        OutFileExtension = outFileExtension;
    }

    private async Task<string> UploadMetadataAsync()
    {
        const string url = "https://convertio.co/process/upload_metadata";

        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(FileId), "file_id");
        formContent.Add(new StringContent(SessionId), "session_id");
        formContent.Add(new StringContent(FileName), "user_fn");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(StringToBytes(FileName)))),
            "user_fn_hash");
        formContent.Add(new StringContent($"{FileData.Length}"), "file_size");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(FileData))), "file_hash");
        formContent.Add(new StringContent(OutFileExtension), "file_out_format");
        formContent.Add(new StringContent(PackId), "pack_id");

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
        var fileContent = new ByteArrayContent(FileData);
        fileContent.Headers.Add("Content-Type", "text/plain");

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(FileId), "file_id");
        content.Add(fileContent, "file", FileName);

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
            new KeyValuePair<string, string>("id", FileId),
            new KeyValuePair<string, string>("type", "convert")
        });

        using var httpClient = new HttpClient();
        
        var response = await httpClient.PostAsync(url, data);
        var dict =
            JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(
                await response.Content.ReadAsStringAsync());
        return dict["convert"][FileId]["out_url"].ToString();
    }

    private async Task<byte[]> DownloadFileAsync(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsByteArrayAsync();
    }
}