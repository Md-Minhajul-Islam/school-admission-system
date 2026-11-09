using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Application.Common.Exceptions;

namespace SchoolAdmission.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request {Method} {Path} was aborted by the client.",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(exception, "Unhandled exception after the response started.");
                throw;
            }

            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail) = Map(exception);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (exception is ValidationException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }

        if (status >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            logger.LogInformation(
                "Handled {ExceptionType} as HTTP {StatusCode}.",
                exception.GetType().Name,
                status);
        }

        context.Response.StatusCode = status;
        context.Response.Headers.Remove("Content-Length");
        context.Response.Headers["X-Correlation-Id"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }

    private static (int Status, string Title, string Detail) Map(Exception exception) => exception switch
    {
        ValidationException => (400, "Validation failed", "Please check the submitted values."),
        NotFoundException => (404, "Not found", exception.Message),
        ForbiddenException => (403, "Forbidden", exception.Message),
        ConflictException => (409, "Conflict", exception.Message),
        DomainException => (400, "Invalid request", exception.Message),
        BadHttpRequestException => (400, "Invalid request", "The request could not be processed."),
        _ => (500, "Server error", "Something went wrong on our side. Please try again.")
    };
}
