using System.Text;
using ConvertShell.Infrastructure;
using static ConvertShell.Utils;


namespace ConvertioClient.Tests;

public class ConvertioClientBuilderTests
{
    [Fact]
    public void UploadMetaDataContent_Test()
    {
        // Arrange
        var metaData = MetaData.Create("fileName",Encoding.UTF8.GetBytes("fileData"), "outFileExtension");

        var contentBuilder = new ConvertioContent();

        // Act
        var result = contentBuilder.UploadMetaDataContent(metaData);

        // Assert
        Assert.NotNull(result);
        
        Assert.IsType<MultipartFormDataContent>(result);
        
        Assert.Equal(metaData.FileId, StringContent(result, "file_id"));
        Assert.Equal(metaData.SessionId, StringContent(result, "session_id"));
        Assert.Equal(metaData.FileName, StringContent(result, "user_fn"));
        Assert.Equal(BytesToString(EncryptMd5(StringToBytes(metaData.FileName))), StringContent(result, "user_fn_hash"));
        Assert.Equal($"{metaData.FileData.Length}", StringContent(result, "file_size"));
        Assert.Equal(BytesToString(EncryptMd5(metaData.FileData)), StringContent(result, "file_hash"));
        Assert.Equal(metaData.OutFileExtension, StringContent(result, "file_out_format"));
        Assert.Equal(metaData.PackId, StringContent(result, "pack_id"));
    }

    [Fact]
    public void UploadFileContent_Test()
    {
        // Arrange
        var metaData = MetaData.Create("fileName",Encoding.UTF8.GetBytes("fileData"), "outFileExtension");

        var contentBuilder = new ConvertioContent();

        // Act
        var result = contentBuilder.UploadFileContent(metaData);

        // Assert
        Assert.NotNull(result);
        
        Assert.IsType<MultipartFormDataContent>(result);
        
        Assert.Equal(metaData.FileData, ByteContent(result, "file"));
        Assert.Equal(metaData.FileId, StringContent(result, "file_id"));
    }

    [Fact]
    public void GetDownloadUrlContent_Test()
    {
        // Arrange
        var metaData = MetaData.Create("fileName",Encoding.UTF8.GetBytes("fileData"), "outFileExtension");
        var contentBuilder = new ConvertioContent();

        // Act
        var result = contentBuilder.GetDownloadUrlContent(metaData);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FormUrlEncodedContent>(result);
        
        AssertEqualKeyValuePair(result, "id", metaData.FileId);
        AssertEqualKeyValuePair(result, "type", "convert");
    }
    
    private void AssertEqualKeyValuePair(FormUrlEncodedContent formContent, string key, string expectedValue)
    {
        var keyValuePairs = formContent.ReadAsStringAsync().Result
            .Split('&')
            .Select(pair => pair.Split('='))
            .ToDictionary(parts => parts[0], parts => parts[1]);

        Assert.Contains(key, keyValuePairs.Keys);
        Assert.Equal(expectedValue, keyValuePairs[key]);
    }

    private string? StringContent(MultipartFormDataContent formData, string name)
    {
        var stringContent = formData.FirstOrDefault(p => p.Headers.ContentDisposition?.Name == name) as StringContent;
        return stringContent?.ReadAsStringAsync().Result;
    }
    
    private byte[]? ByteContent(MultipartFormDataContent formData, string name)
    {
        var byteContent = formData.FirstOrDefault(p => p.Headers.ContentDisposition?.Name == name) as ByteArrayContent;
        return byteContent?.ReadAsByteArrayAsync().Result;
    }
}