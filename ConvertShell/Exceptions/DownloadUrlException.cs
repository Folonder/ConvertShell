namespace ConvertShell.Exceptions;

public class DownloadUrlException : Exception
{
    public DownloadUrlException()
    {
    }

    public DownloadUrlException(string message) : base(message)
    {
    }

    public DownloadUrlException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
