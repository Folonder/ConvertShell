namespace ConvertShell.Infrastructure;

public abstract class RetryParams
{
    public abstract int MaxTries { get; set; }

    public abstract TimeSpan RetryInterval { get; set; }
}