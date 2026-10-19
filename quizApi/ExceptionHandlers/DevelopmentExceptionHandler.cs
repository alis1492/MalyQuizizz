using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;
using quizShared.DTOs;
using quizShared;
using quizApi.Exceptions;

namespace quizApi.ExceptionHandlers;

// Logs everything and returns informative error messages to help frontend team
public class DevelopmentExceptionHandler(
    ILogger<DevelopmentExceptionHandler> logger
) : IExceptionHandler
{
    private readonly ILogger<DevelopmentExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = 400;
        httpContext.Response.ContentType = "application/problem+json";
        if(exception is FluentValidation.ValidationException || exception is ValidationException)
        {
            var payload = new DevelopmentExceptionDTO("Validation Error", 400, exception);
            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }
        else if(exception is DocumentNotFoundException)
        {
            var payload = new DevelopmentExceptionDTO("Not Found", 404, exception);
            httpContext.Response.StatusCode = 404;
            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        } 
        else if(exception is MongoException)
        {
            var payload = new DevelopmentExceptionDTO("Database Error", 500, exception);
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }

        // * true means handled
        // * false means unhandled - rethrow
        return false;
    }
}