using Xunit.Abstractions;

namespace Convert.IntegrationTests.ConvertControllerTests;

public class ConvertControllerTests : TestBase
{
    private const string FileName = "happy.txt";
    
    private readonly MetaData _metaData = MetaData.Create(FileName, "Happy flow"u8.ToArray(), "PDF");
    
    public ConvertControllerTests(ITestOutputHelper output) : base(output)
    {
    }
    
    [Fact]
    public async Task FullTest()
    {
        #region Arrange

        //Load file
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody($"{_application.BaseAddress}/process/upload_file"));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_file").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("OK"));
        
        _application.ApiMockServer.Given(Request.Create().WithPath("/process/get_file_status").UsingPost()).InScenario("FullTest").WillSetStateTo("FullTestStep2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(FileStatusUndoneJson(_metaData.FileId)));
        
        _application.ApiMockServer.Given(Request.Create().WithPath("/process/get_file_status").UsingPost()).InScenario("FullTest").WhenStateIs("FullTestStep2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(FileStatusDoneJson(_metaData.FileId)));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/download_url").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyFromFile(GetFilePath("happy.pdf")));

        #endregion

        #endregion

        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("OK", result.StatusCode.ToString());
        Assert.Equal("application/pdf", result.Content.Headers.ContentType?.MediaType);
        Assert.Equal(FileName, result.Content.Headers.ContentDisposition?.FileName);
        Assert.Equal(await File.ReadAllBytesAsync(GetFilePath("happy.pdf")), await result.Content.ReadAsByteArrayAsync());

        #endregion
    }
    
    [Fact]
    public async Task EndOfConvertTimeTest()
    {
        #region Arrange
        
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("No minutes left"));

        #endregion

        #endregion
        
        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("BadRequest", result.StatusCode.ToString());
        Assert.Equal("{\"message\":\"No minutes left\"}", await result.Content.ReadAsStringAsync());
        #endregion
    }
    
    [Fact]
    public async Task MalformedRequestTest()
    {
        #region Arrange
        
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("Malformed request"));

        #endregion

        #endregion
        
        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("BadRequest", result.StatusCode.ToString());
        Assert.Equal("{\"message\":\"Malformed request\"}", await result.Content.ReadAsStringAsync());
        #endregion
    }

    [Fact]
    public async Task NotAllowedExtensionTest()
    {
        #region Arrange

        //Load file
        var fileName = "happy.pdf";
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(fileName)), fileName);

        #endregion
        
        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("BadRequest", result.StatusCode.ToString());
        Assert.Equal("File extension must be one of these: .txt", await result.Content.ReadAsStringAsync());
        #endregion
    }

    [Fact]
    public async Task UploadFileNotOkTest()
    {
        #region Arrange

        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody($"{_application.BaseAddress}/process/upload_file"));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_file").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("Not OK"));

        #endregion

        #endregion

        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("BadRequest", result.StatusCode.ToString());
        Assert.Equal("{\"message\":\"Not OK\"}", await result.Content.ReadAsStringAsync());
        
        #endregion
    }

    [Fact]
    public async Task MaxRetriesExceededTest()
    {
        #region Arrange
        
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody($"{_application.BaseAddress}/process/upload_file"));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_file").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("OK"));
        
        _application.ApiMockServer.Given(Request.Create().WithPath("/process/get_file_status").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(FileStatusUndoneJson(_metaData.FileId)));

        #endregion

        #endregion

        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert

        Assert.NotNull(result);
        Assert.Equal("BadRequest", result.StatusCode.ToString());
        Assert.Equal("{\"message\":\"Can not get download url\"}", await result.Content.ReadAsStringAsync());

        #endregion
    }

    [Fact]
    public async Task WithoutRetriesTest()
    {
        #region Arrange
        
        var multipartFormContent = GetContentFromTxtFile(File.OpenRead(GetFilePath(FileName)), FileName);
        
        #region WireMock

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_metadata").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody($"{_application.BaseAddress}/process/upload_file"));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/upload_file").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("OK"));
        
        _application.ApiMockServer.Given(Request.Create().WithPath("/process/get_file_status").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(FileStatusDoneJson(_metaData.FileId)));

        _application.ApiMockServer.Given(Request.Create().WithPath("/process/download_url").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyFromFile(GetFilePath("happy.pdf")));

        #endregion

        #endregion

        #region Act

        var client = _application.CreateClient();

        var result = await client.PostAsync("/api/convert/to-pdf", multipartFormContent);
        
        #endregion

        #region Assert
        
        Assert.NotNull(result);
        Assert.Equal("OK", result.StatusCode.ToString());
        Assert.Equal("application/pdf", result.Content.Headers.ContentType?.MediaType);
        Assert.Equal(FileName, result.Content.Headers.ContentDisposition?.FileName);

        Assert.Equal(await File.ReadAllBytesAsync(GetFilePath("happy.pdf")), await result.Content.ReadAsByteArrayAsync());

        #endregion 
    }
    
    protected override void AddCustomServicesConfiguration(IServiceCollection services)
    {
        services.Configure<ConvertioConverterOptions>(options =>
        {
            options.UploadMetaDataUrl = $"{_application.BaseAddress}/process/upload_metadata";
            options.GetFileUrl = $"{_application.BaseAddress}/process/get_file_status";
        });
        
        var convertioContent = new Mock<ConvertioContent>();
        convertioContent.Setup(content => content.MetaData(_metaData.FileName, It.IsAny<byte[]>(), _metaData.OutFileExtension))
            .Returns(_metaData);
        services.AddScoped<ConvertioContent>(_ => convertioContent.Object);
    }
    
    private string GetFilePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."))
               + @"\ConvertControllerTests\TestFiles\" + fileName;
    }

    private MultipartFormDataContent GetContentFromTxtFile(Stream content, string fileName)
    {
        var multipartFormContent = new MultipartFormDataContent();

        var fileStreamContent = new StreamContent(content);

        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

        multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

        return multipartFormContent;
    }

    private string FileStatusDoneJson(string fileId)
    {
        return $$"""
                  {
                    "upload": {
                      "{{fileId}}": {
                        "done": true,
                        "error": ""
                      }
                    },
                    "convert": {
                      "{{fileId}}": {
                        "done": true,
                        "error": "",
                        "out_url": "{{_application.BaseAddress}}/process/download_url",
                        "out_size": 73221,
                        "credits": 0,
                        "percent": 0
                      }
                    }
                  }
                 """;
    }

    private string FileStatusUndoneJson(string fileId)
    {
        return $$"""
                    {
                      "upload": {
                        "{{fileId}}": {
                          "done": true,
                          "error": ""
                        }
                      },
                      "convert": {
                        "{{fileId}}": {
                          "done": false,
                          "error": "",
                          "out_url": "",
                          "out_size": 0,
                          "credits": 0,
                          "percent": 0
                        }
                      }
                    }
                 """;
    }
}