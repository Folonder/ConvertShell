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
    public async Task<IActionResult> ToPdf(IFormFile? file)
    {
        try
        {
            var convertedFile = await _converterService.ToPdf(file);

            return File(convertedFile, "application/pdf", file.FileName);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}


