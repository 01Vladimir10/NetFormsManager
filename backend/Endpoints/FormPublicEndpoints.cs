using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetMailGun.Api;
using NetMailGun.Api.Validators;
using NetMailGun.Core.Model;
using NetMailGun.Core.Repositories;
using NetMailGun.Core.Services;

namespace NetMailGun.Endpoints;

public static class FormPublicEndpoints
{
    public static void MapFormPublicEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/forms/{formId:guid:}/fields", async (Guid formId, IFormsRepository formsRepository) =>
            {
                var form = await formsRepository.FindByIdAsync(formId);
                return form is null ? ErrorResults.NotFound() : Results.Ok(form.Fields);
            })
            .AllowAnonymous();

        builder.MapPost(
                pattern: "/forms/{formId:guid}/submit",
                handler: async (
                    [FromRoute] Guid formId,
                    [FromQuery] string? token,
                    ISubscriptionProviderFactory subscriptionProviderFactory,
                    IFormsRepository formsRepository,
                    IBotValidatorFactory botValidationFactory,
                    IEmailTemplatesRepository templatesRepository,
                    IEmailService emailService,
                    ITemplateRendererService templateRendererService,
                    HttpContext context,
                    ILoggerFactory loggerFactory
                ) =>
                {
                    var logger = loggerFactory.CreateLogger("FormsSubmit");
                    // var logger = loggerFactory.CreateLogger("FormSubmit");
                    var form = await formsRepository.FindByIdAsync(formId);

                    if (form == null)
                        return ErrorResults.NotFound();

                    var (payloadErrorResult, payload , rawPayload) = await ReadPayload(form, context.Request);

                    if (payloadErrorResult is not null)
                        return payloadErrorResult;

                    // try to get the token from the body if it was not provided via a query param.
                    token ??= rawPayload.GetValueOrDefault("token") ?? rawPayload.GetValueOrDefault("$token");

                    if (string.IsNullOrWhiteSpace(token))
                        return ErrorResults.BadRequest("Invalid token");

                    var refererValidationResult = ValidateFormReferer(form, context.Request);

                    if (refererValidationResult is not null)
                        return refererValidationResult;

                    var botValidatorResult = await ValidateFormBot(form, token, botValidationFactory, context);
                    if (botValidatorResult is not null)
                        return botValidatorResult;

                    await ExecuteSubscriptionAsync(form, payload, subscriptionProviderFactory);

                    await SendFormEmails(formId, payload, templatesRepository, emailService, templateRendererService);

                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Form submitted, formId: {FormId}, referer: {Referer}", formId,
                            context.Request.Headers.Referer);
                    }

                    return Results.Ok();
                })
            .AllowAnonymous()
            .Accepts(typeof(Dictionary<string, string>), MediaTypeNames.Application.FormUrlEncoded,
                MediaTypeNames.Application.Json)
            .Produces(StatusCodes.Status200OK, typeof(void))
            .Produces<ErrorDto>(StatusCodes.Status500InternalServerError)
            .Produces<ErrorDto>(StatusCodes.Status404NotFound)
            .Produces<ErrorDto>(StatusCodes.Status400BadRequest);

        builder.MapMethods(
                pattern: "/forms/{formId:guid}/submit",
                httpMethods: [HttpMethods.Options],
                handler: ([FromRoute] Guid formId, IFormsRepository formsRepository, HttpContext context)
                    => HandleDynamicCorsResponse(formId, formsRepository, context, HttpMethods.Post)
            )
            .AllowAnonymous();

        builder.MapMethods(
                pattern: "/forms/{formId:guid}/fields",
                httpMethods: [HttpMethods.Options],
                handler: ([FromRoute] Guid formId, IFormsRepository formsRepository, HttpContext context)
                    => HandleDynamicCorsResponse(formId, formsRepository, context, HttpMethods.Get)
            )
            .AllowAnonymous();
    }

    private static async Task<IResult?> ValidateFormBot(FormEntity form, string token,
        IBotValidatorFactory botValidatorFactory, HttpContext context)
    {
        if (form.BotValidator is null) return null;

        if (botValidatorFactory.TryCreate(form.BotValidator.Provider, out var validator))
        {
            if (!await validator.ValidateAsync(token, form.BotValidator.Parameters, context))
                return ErrorResults.BadRequest("Token validation failed");
        }
        else
            return ErrorResults.BadRequest(
                "Token validation failed, unsupported token validation service.");

        return null;
    }

    private static async Task<(IResult? httpResult, Dictionary<string, object?> payload, Dictionary<string, string?> rawPayload)> ReadPayload(
        FormEntity form,
        HttpRequest request)
    {
        var body = await ReadBodyAsDictionary(request);
        if (body == null)
        {
            return (httpResult: ErrorResults.BadRequest("Failed to parse payload"), payload: [], rawPayload: []);
        }

        if (!FormPayloadParser.TryParse(form.Fields, body, out var payload, out var errors))
        {
            return (httpResult: ErrorResults.InvalidRequest(errors), payload: [], rawPayload: []);
        }

        return (httpResult: null, payload, body);

        static async Task<Dictionary<string, string?>?> ReadBodyAsDictionary(HttpRequest request)
        {
            try
            {
                if (request.ContentType?.StartsWith(MediaTypeNames.Application.FormUrlEncoded) ?? false)
                {
                    return request.Form.ToDictionary(
                        x => x.Key,
                        string? (x) => x.Value.ToString()
                    );
                }

                var dict = await JsonSerializer.DeserializeAsync<JsonElement>(request.Body, JsonSerializerOptions.Web);

                if (dict.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                return dict.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.ValueKind switch
                {
                    JsonValueKind.String => x.Value.GetString(),
                    JsonValueKind.Number => x.Value.GetDecimal().ToString(CultureInfo.InvariantCulture),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                });
            }
            catch
            {
                return null;
            }
        }
    }

    private static IResult? ValidateFormReferer(FormEntity form, HttpRequest request)
    {
        var referer = request.GetTypedHeaders().Referer;
        return referer is not null
               && (form.AllowedOrigins is ["*"] ||
                   form.AllowedOrigins.Any(x => referer.Host.Equals(x, StringComparison.OrdinalIgnoreCase)))
            ? null
            : ErrorResults.BadRequest(
                "Not allowed",
                $"'{referer?.Authority}' is not allowed to submit this form"
            );
    }


    private static async Task SendFormEmails(
        Guid formId,
        Dictionary<string, object?> payload,
        IEmailTemplatesRepository templatesRepository,
        IEmailService emailService,
        ITemplateRendererService templateRendererService
    )
    {
        var templates = await templatesRepository.GetByFormIdAsync(formId);

        if (templates.Count == 0) return;

        await emailService.SendAsync(templates.Select(template => new EmailMessage
        {
            To = template.To.Select(x => templateRendererService.Render(x, payload)).ToArray(),
            Bcc = template.Bcc?.Select(x => templateRendererService.Render(x, payload)).ToArray(),
            Cc = template.Cc?.Select(x => templateRendererService.Render(x, payload)).ToArray(),
            ReplyTo = template.ReplyTo,
            Body = templateRendererService.Render(template.Body, payload),
            Subject = templateRendererService.Render(template.SubjectTemplate, payload)
        }));
    }

    private static async Task ExecuteSubscriptionAsync(
        FormEntity form,
        Dictionary<string, object?> payload,
        ISubscriptionProviderFactory subscriptionProviderFactory)
    {
        if (form.Subscription is null ||
            !subscriptionProviderFactory.TryCreate(form.Subscription.Provider, out var subscriptionProvider)) return;
        
        var emailAddress = payload.GetValueOrDefault(form.Subscription.FieldReferences.Email)?.ToString();
        
        var name = string.IsNullOrWhiteSpace(form.Subscription.FieldReferences.Name)
            ? null
            : payload.GetValueOrDefault(form.Subscription.FieldReferences.Name)?.ToString();

        var lastName = string.IsNullOrWhiteSpace(form.Subscription.FieldReferences.Lastname)
            ? null
            : payload.GetValueOrDefault(form.Subscription.FieldReferences.Lastname)?.ToString();

        var phone = string.IsNullOrWhiteSpace(form.Subscription.FieldReferences.Phone)
            ? null
            : payload.GetValueOrDefault(form.Subscription.FieldReferences.Phone)?.ToString();

        if (!emailAddress.IsValidEmailAddress())
        {
            return;
        }

        await subscriptionProvider.SubscribeAsync(
            formId: form.Id,
            emailAddress: emailAddress,
            name: name,
            lastName: lastName,
            phoneNumber: phone
        );
    }

    private static async Task<IResult> HandleDynamicCorsResponse(Guid formId, IFormsRepository formsRepository,
        HttpContext context, string method)
    {
        var form = await formsRepository.FindByIdAsync(formId);
        if (form is null) return Results.Empty;
        var headers = context.Request.GetTypedHeaders();
        var host = headers.Referer?.Host;
        var allowedOrigin = form.AllowedOrigins switch
        {
            ["*"] => "*",
            _ when host is not null &&
                   form.AllowedOrigins.Any(x => x.Equals(host, StringComparison.OrdinalIgnoreCase))
                => host,
            _ => null
        };
        if (allowedOrigin is null) return Results.Empty;
        context.Response.Headers.AccessControlAllowOrigin = allowedOrigin;
        context.Response.Headers.AccessControlAllowHeaders = method;
        return Results.Empty;
    }
}