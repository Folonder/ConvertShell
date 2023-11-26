namespace ConvertShell.Infrastructure;

public class ConvertioClientOptions
{
    public const string Key = "ConvertioClient";
    public virtual string UploadMetaDataUrl { get; set; }
    public virtual string GetFileUrl { get; set; }
}