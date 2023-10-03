using System.Text.Json;
using static ConvertShell.Utils;

namespace ConvertShell.Infrastructure;

public class ConvertioConverter : IConverter
{
    private string FileId { get; set; }

    private string PackId { get; set; }

    private string SessionId { get; set; }

    private byte[] FileName { get; set; }

    private byte[] FileData { get; set; }

    public async Task<byte[]> ConvertAsync(byte[] fileName, byte[] fileData)
    {
        UpdateMetaData(fileName, fileData);
        string uploadFileUrl = await UploadMetadataAsync();
        await UploadFileAsync(uploadFileUrl);

        Thread.Sleep(7000);

        string downloadUrl = await GetFileStatusAsync();
        return await DownloadFileAsync(downloadUrl);
    }


    private void UpdateMetaData(byte[] fileName, byte[] fileData)
    {
        FileId = RandomString(32);
        PackId = RandomString(6);
        SessionId = RandomString(32);

        FileName = fileName;
        FileData = fileData;
    }

    private async Task<string> UploadMetadataAsync()
    {
        string url = "https://convertio.co/process/upload_metadata";

        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(FileId), "file_id");
        formContent.Add(new StringContent(SessionId), "session_id");
        formContent.Add(new StringContent(BytesToString(FileName)), "user_fn");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(FileName))), "user_fn_hash");
        formContent.Add(new StringContent(""), "file_source");
        formContent.Add(new StringContent($"{FileData.Length}"), "file_size");
        formContent.Add(new StringContent(BytesToString(EncryptMd5(FileData))), "file_hash");
        formContent.Add(new StringContent("PDF"), "file_out_format");
        formContent.Add(new StringContent(PackId), "pack_id");
        formContent.Add(new StringContent("{}"), "settings");
        formContent.Add(new StringContent(""), "send_db_token");
        formContent.Add(new StringContent(""), "send_gd_token");

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(url, formContent);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent == "Malformed request" || responseContent == "No minutes left")
                {
                    throw new Exception(responseContent);
                }
                return responseContent;
            }
            throw new Exception(response.ToString());
        }
    }

    private async Task UploadFileAsync(string url)
    {
        using (var httpClient = new HttpClient())
        {
            var fileContent = new ByteArrayContent(FileData);
            fileContent.Headers.Add("Content-Type", "text/plain");

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(FileId), "file_id");
            content.Add(fileContent, "file", BytesToString(FileName));

            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != "OK")
                {
                    throw new Exception(responseContent);
                }
            }
            else
            {
                throw new Exception(response.ToString());
            }
        }
    }


    private async Task<string> GetFileStatusAsync()
    {
        string url = "https://convertio.co/process/get_file_status";
        
        using (var httpClient = new HttpClient())
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", FileId),
                new KeyValuePair<string, string>("type", "convert")
            });

            var response = await httpClient.PostAsync(url, data);
            var dict =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, object>>>>(
                    await response.Content.ReadAsStringAsync());
            return dict["convert"][FileId]["out_url"].ToString();
        }
    }

    private async Task<byte[]> DownloadFileAsync(string url)
    {
        using (var httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}