using static ConvertShell.Infrastructure.ConvertioContentUtils;

namespace ConvertShell.Infrastructure;

public class ConvertioContent
{
    public virtual MetaData MetaData(string fileName, byte[] fileData, string outFileExtension)
    {
        return Infrastructure.MetaData.Create(fileName, fileData, outFileExtension);
    }

    public MultipartFormDataContent UploadMetaDataContent(MetaData metaData)
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

    public MultipartFormDataContent UploadFileContent(MetaData metaData)
    {
        var fileContent = new ByteArrayContent(metaData.FileData);
        fileContent.Headers.Add("Content-Type", "text/plain");
 
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(metaData.FileId), "file_id");
        content.Add(fileContent, "file", metaData.FileName);
        return content;
    }

    public FormUrlEncodedContent GetDownloadUrlContent(MetaData metaData)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id", metaData.FileId),
            new KeyValuePair<string, string>("type", "convert")
        });
        return content;
    }
}