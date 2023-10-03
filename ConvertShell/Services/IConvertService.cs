namespace ConvertShell.Services
{
    public interface IConvertService
    {
        public Task<byte[]> ToPdf(IFormFile? file);
    }
}
