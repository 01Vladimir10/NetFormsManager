using System.Diagnostics;
using FluentValidation.Results;

namespace NetFormsManager.Api;

public class ErrorDto
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? Cause { get; init; }
    public Dictionary<string, List<string>>? Errors { get; init; }

    public string? TraceId { get; } = Activity.Current?.TraceId.ToString();
    public static ErrorDto InvalidRequest(Dictionary<string, List<string>> validationErrors) => new()
    {
        Code = "INVALID_REQUEST",
        Message = "Invalid request",
        Cause = "The payload you provided is invalid. Please check the errors provided and try again.",
        Errors = validationErrors
    };
    
    public static ErrorDto InvalidRequest(string message, string? cause = null) => new()
    {
        Code = "INVALID_REQUEST",
        Message = message,
        Cause = cause
    };

    public static ErrorDto Unauthorized() => new()
    {
        Code = "UNAUTHORIZED",
        Message = "You are not authorized to access this resource",
        Cause = "Invalid authentication token"
    };

    public static ErrorDto InternalServerError() => new()
    {
        Code = "INTERNAL_SERVER_ERROR",
        Message = "Internal server error, please try again later"
    };

    public static ErrorDto NotFound() => new()
    {
        Code = "NOT_FOUND",
        Message = "Resource not found",
    };
}

public static class ErrorResults
{
    public static IResult NotFound() => Results.NotFound(ErrorDto.NotFound());
    public static IResult InvalidRequest(ValidationResult validationErrors) =>
        Results.BadRequest(ErrorDto.InvalidRequest(
                validationErrors.Errors
                    .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                    .ToDictionary(x => x.Key, x => x.ToList())
            )
        );
    
    public static IResult InvalidRequest(Dictionary<string, List<string>> validationErrors) =>
        Results.BadRequest(ErrorDto.InvalidRequest(validationErrors));
    public static IResult BadRequest(string error, string? cause = null) =>
        Results.BadRequest(ErrorDto.InvalidRequest(error, cause));
}