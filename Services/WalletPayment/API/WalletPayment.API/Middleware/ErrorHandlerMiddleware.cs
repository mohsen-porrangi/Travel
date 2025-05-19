using BuildingBlocks.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace WalletPayment.API.Middleware;

public class ErrorHandlerMiddleware : IExceptionHandler
{
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "خطا در پردازش درخواست: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            BadRequestException ex => CreateProblemDetails(ex.Message, HttpStatusCode.BadRequest, ex.Details),
            NotFoundException ex => CreateProblemDetails(ex.Message, HttpStatusCode.NotFound, null),
            ForbiddenDomainException ex => CreateProblemDetails(ex.Message, HttpStatusCode.Forbidden, null),
            UnauthorizedDomainException ex => CreateProblemDetails(ex.Message, HttpStatusCode.Unauthorized, null),
            ValidationException validationEx => CreateValidationProblemDetails(validationEx),
            _ => CreateProblemDetails("یک خطای داخلی در سرور رخ داده است", HttpStatusCode.InternalServerError, null)
        };

        context.Response.StatusCode = (int)problemDetails.Status!;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(string title, HttpStatusCode statusCode, string? detail)
    {
        return new ProblemDetails
        {
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = Guid.NewGuid().ToString()
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(ValidationException validationException)
    {
        // برای FluentValidation، خطاها به صورت زیر استخراج می‌شوند
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Title = "یک یا چند خطای اعتبارسنجی رخ داده است",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "لطفاً اطلاعات ورودی را اصلاح کنید و دوباره تلاش کنید",
            Instance = Guid.NewGuid().ToString()
        };
    }
}