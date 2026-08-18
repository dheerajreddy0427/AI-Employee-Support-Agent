using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSupportAgent.API.Infrastructure;

/// <summary>
/// Maps unhandled exceptions to RFC 7807 ProblemDetails responses so the
/// frontend can pull a meaningful `detail`/`title` from any error.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _log;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> log)
    {
        _log = log;
    }

    public void OnException(ExceptionContext context)
    {
        var http = context.HttpContext;
        var (status, title, detail) = Map(context.Exception);

        _log.Log(
            status >= 500 ? LogLevel.Error : LogLevel.Warning,
            context.Exception,
            "Handled exception ({Status}): {Title}",
            status, title);

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = http.Request.Path
        };

        if (context.Exception is ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors;
        }

        context.Result = new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
        context.ExceptionHandled = true;
    }

    private static (int status, string title, string detail) Map(Exception ex) => ex switch
    {
        ValidationException v => (StatusCodes.Status400BadRequest, "Validation failed", v.Message),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "You are not authorized to perform this action."),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found", ex.Message),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", "The record was modified by someone else. Reload and try again."),
        InvalidOperationException ioe => (StatusCodes.Status400BadRequest, "Invalid operation", ioe.Message),
        ArgumentException ae => (StatusCodes.Status400BadRequest, "Invalid argument", ae.Message),
        _ => (StatusCodes.Status500InternalServerError, "Server error", "An unexpected error occurred.")
    };
}

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
