﻿using System.Net;
using System.Text.Json;
using ConvertShell.Exceptions;

namespace ConvertShell.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            switch(error)
            {
                case ArgumentException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case WebException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case DownloadUrlException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case KeyNotFoundException e:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            var result = JsonSerializer.Serialize(new { message = error?.Message });
            await response.WriteAsync(result);
        }
    }
}