using ConvertShell.Attributes;
using ConvertShell.Extensions;
using ConvertShell.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConvertShell.Controllers;


[Route("api/convert")]
public class ConvertController : ControllerBase
{
    private readonly ILogger<ConvertController> _logger;

    private readonly IConvertService _converterService;
    
    public ConvertController(ILogger<ConvertController> logger, IConvertService converterService)
    {
        _logger = logger;
        _converterService = converterService;
    }

    [HttpPost]
    [Route("to-pdf")]
    [AllowedExtensions(new string[] {".txt"})]
    [FileSizeRange(1, 100 * 1024 * 1024)]
    public async Task<IActionResult> ToPdf(IFormFile file)
    {
        var convertedFile = await _converterService.ConvertFile(file.FileName, await file.ReadFile(), "PDF");
        return File(convertedFile, "application/pdf", file.FileName.ChangeFileExtension("pdf"));
    }
}


