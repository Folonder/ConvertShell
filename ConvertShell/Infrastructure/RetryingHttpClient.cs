using ConvertShell.Exceptions;
using Polly;
using Polly.Retry;
 
namespace ConvertShell.Infrastructure;
 
public class RetryingHttpClient
{
    public readonly AsyncRetryPolicy _retryPolicy;
 
    public RetryingHttpClient(RetryParams retryParams)
    {
        _retryPolicy = Policy
            .Handle<DownloadUrlException>() 
            .WaitAndRetryAsync(retryParams.MaxTries, retryAttempt => retryParams.RetryInterval, (exception, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry #{retryCount}: {exception.Message}, retrying in {timeSpan.TotalSeconds} seconds");
            });
    }
 
    public async Task<string> ExecuteWithRetryIfResultEmpty(Func<Task<string>> action)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            string result = await action();
 
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
 
            throw new DownloadUrlException("Empty response, retrying...");
        });
    }
}