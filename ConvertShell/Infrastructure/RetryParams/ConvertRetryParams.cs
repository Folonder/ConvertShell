namespace ConvertShell.Infrastructure;

public class ConvertRetryParams : RetryParams
{
    public override int MaxTries { get ; set; }

    public override TimeSpan RetryInterval { get; set; }

    public ConvertRetryParams(int fileSize, string outFileExtension)
    {
        CalcConvertRetryParams(fileSize, outFileExtension);
    }

    private void CalcConvertRetryParams(int fileSize, string outFileExtension)
    {
        MaxTries = 10;
        RetryInterval = TimeSpan.FromSeconds(3);
    }
}