using ConvertShell.Attributes;
using ConvertShell.Services;
using Microsoft.AspNetCore.Mvc;
using static ConvertShell.Utils;

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
    [FileSizeRange(1, 100 * 1024 * 1024)]
    [AllowedExtensions(new string[] {".txt"})]
    public async Task<IActionResult> ToPdf(IFormFile file)
    {
        var convertedFile = await _converterService.ConvertFile(file.FileName, await ReadFile(file), ".PDF");
        return File(convertedFile, "application/pdf", file.FileName);
    }
}


