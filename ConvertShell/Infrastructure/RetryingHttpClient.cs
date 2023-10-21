using Polly;
using Polly.Retry;
 
namespace ConvertShell.Infrastructure;
 
public class RetryingHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _retryPolicy;
 
    public RetryingHttpClient(RetryParams retryParams)
    {
        var maxRetries = retryParams.MaxTries;
        var retryInterval = retryParams.RetryInterval;
        _httpClient = new HttpClient();
        _retryPolicy = Policy
            .Handle<Exception>() 
            .WaitAndRetryAsync(maxRetries, retryAttempt => retryInterval, (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry #{retryCount}: {exception.Message}, retrying in {timeSpan.TotalSeconds} seconds");
            });
    }
 
    public async Task<string> ExecuteWithRetry(Func<Task<string>> action)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            string result = await action();
 
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
 
            throw new Exception("Empty response, retrying...");
        });
    }
}