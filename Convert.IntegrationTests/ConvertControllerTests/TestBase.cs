using Xunit.Abstractions;

namespace Convert.IntegrationTests.ConvertControllerTests;

public abstract class TestBase: IDisposable
{
    internal readonly ConvertShellApplication _application;
    internal readonly HttpClient _client;

    public TestBase(ITestOutputHelper output)
    {
        _application = new ConvertShellApplication(output);
        _application.CustomServicesConfiguration += _application_CustomServicesConfiguration;
        _client = _application.CreateClient();
    }

    private void _application_CustomServicesConfiguration(object? sender, IServiceCollection services)
    {
        AddCustomServicesConfiguration(services);
    }

    protected virtual void AddCustomServicesConfiguration(IServiceCollection services)
    { }

    public virtual void Dispose()
    {
        _client?.Dispose();
        _application?.StopIntegration();
        _application?.Dispose();
    }
}
