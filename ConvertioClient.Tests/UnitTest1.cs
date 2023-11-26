using System.Net;
using System.Text;
using ConvertShell.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

public class ConvertioClientTests
{
    [Fact]
    public async Task ConvertAsync_ShouldPerformConversion()
    {
        // Arrange

        var optionsMock = new Mock<IOptions<ConvertioClientOptions>>();
        optionsMock.SetupGet(c => c.Value.UploadMetaDataUrl).Returns("https://convertio.co/process/upload_metadata");
        optionsMock.SetupGet(c => c.Value.GetFileUrl).Returns("https://convertio.co/process/get_file_status");

        var mockedMetadata = new Mock<MetaData>();

        var metadata = new MetaData().Create("convert file name", Encoding.UTF8.GetBytes("test string"), "PDF");


        mockedMetadata.Setup(c => c.Create(It.IsAny<string>(),
                It.IsAny<byte []>(), It.IsAny<string>()))
            .Returns(metadata);
        
        
        var mockMessageHandler = new Mock<HttpMessageHandler>();

        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post &&
                request.RequestUri == new Uri("https://convertio.co/process/upload_metadata")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("https://google.com")
            });

        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post &&
                request.RequestUri == new Uri("https://google.com")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("OK")
            });
        
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri == new Uri("https://convertio.co/process/get_file_status")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"upload\":{\"444d64f84960cbc40ba1f3d3d2f940e9\":{\"done\":true,\"error\":\"\"}},\"convert\":{\"" + metadata.FileId + "\":{\"done\":true,\"error\":\"\",\"out_url\":\"https://s188.convertio.me/p/HKZBAZ5GAonaRHLGqqU3Kg/aa3e2279113f80a4ac0e1ff24276ae50/test_convertio.pdf\",\"out_size\":50771,\"credits\":0,\"percent\":0}}}")
            });  
        
        mockMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Get &&
                    request.RequestUri == new Uri("https://s188.convertio.me/p/HKZBAZ5GAonaRHLGqqU3Kg/aa3e2279113f80a4ac0e1ff24276ae50/test_convertio.pdf")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                Content = new StringContent("converted file")
            });

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(mockMessageHandler.Object));

        
        var convertioClient =
            new ConvertShell.Infrastructure.ConvertioClient(optionsMock.Object, mockHttpClientFactory.Object, mockedMetadata.Object);
        
        //Act

        await convertioClient.ConvertAsync("convert file name", Encoding.UTF8.GetBytes("test string"), "PDF");
        
        //Assert

        
    }
}