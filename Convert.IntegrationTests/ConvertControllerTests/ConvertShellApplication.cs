using ConvertShell;
using Microsoft.Extensions.Hosting;
using WireMock.Server;
using Xunit.Abstractions;

namespace Convert.IntegrationTests.ConvertControllerTests;

internal class ConvertShellApplication : WebApplicationFactory<Program>
{
    protected ITestOutputHelper Output;
    
    private const int ApiTestPort = 8001;
    public readonly string BaseAddress = $"http://localhost:{ApiTestPort}";
    
    internal WireMockServer ApiMockServer { get; private set; }

    public ConvertShellApplication(ITestOutputHelper output)
    {
        Output = output;
    }

    private void StartIntegration()
    {
        ApiMockServer = WireMockServer.Start(ApiTestPort);
    }

    internal void StopIntegration()
    {
        ApiMockServer.Stop();
        ApiMockServer.Dispose();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        
        builder.ConfigureServices(services =>
        {
            CustomServicesConfiguration?.Invoke(this, services);
        });

        StartIntegration();

        return base.CreateHost(builder);
    }

    internal event EventHandler<IServiceCollection> CustomServicesConfiguration;
    

    protected override void Dispose(bool disposing)
    {
        StopIntegration();
        base.Dispose(disposing);
    }
}