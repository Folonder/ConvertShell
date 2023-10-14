using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConvertShell.Attributes;

public class FileSizeRangeAttribute : ActionFilterAttribute
{
    private readonly int _minFileSize;
    private readonly int _maxFileSize;
    public FileSizeRangeAttribute(int minFileSize, int maxFileSize)
    {
        _minFileSize = minFileSize;
        _maxFileSize = maxFileSize;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("file", out var parameterValue) && parameterValue is IFormFile file)
        {
            if (file.Length > _maxFileSize || file.Length < _minFileSize)
            {
                context.Result = new BadRequestObjectResult(GetErrorMessage());
            }
        }
        base.OnActionExecuting(context);
    }

    private string GetErrorMessage()
    {
        return $"Allowed file size range is [{_minFileSize}, {_maxFileSize}] bytes.";
    }
}