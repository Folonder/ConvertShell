using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConvertShell.Attributes;

public class AllowedExtensionsAttribute : ActionFilterAttribute
{
    private readonly string[] _extensions;
    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("file", out var parameterValue) && parameterValue is IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
            {
                context.Result = new BadRequestObjectResult(GetErrorMessage());
            }
        }
        base.OnActionExecuting(context);
    }

    private string GetErrorMessage()
    {
        return $"File extension must be one of these: {string.Join(", ", _extensions)}";
    }
}