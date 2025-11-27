using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NetFormsManager.Api;
using NetFormsManager.Core.Repositories;
using NetFormsManager.Core.Services;
using NetFormsManager.Utils;

namespace NetFormsManager.Endpoints;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriberEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/forms/{formId:guid}/subscribers", async (
            Guid formId,
            IFormsRepository formsRepository,
            ISubscriptionProviderFactory subscriptionProviderFactory,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100
        ) =>
        {
            var form = await formsRepository.FindByIdAsync(formId);
            if (form == null)
            {
                return ErrorResults.NotFound();
            }

            if (pageSize is <= 0 or > 500)
            {
                return ErrorResults.BadRequest("Invalid page size value, value must be between 1 and 500.");
            }

            if (page < 1)
            {
                return ErrorResults.BadRequest("Invalid page value, value must be greater than or equal to 1.");
            }

            if (form.Subscription is null)
            {
                return ErrorResults.BadRequest(
                    error: "This form does not support subscriptions",
                    cause: "Please set a subscription provider first"
                );
            }

            if (!subscriptionProviderFactory.TryCreate(form.Subscription.Provider, out var provider))
            {
                throw new Exception(
                    $"Invalid configuration exception, the form {formId} references a non-existing subscription provider: {form.Subscription.Provider}"
                );
            }

            return Results.Ok(await provider.GetSubscribersAsync(formId, page, pageSize));
        });

        endpoints.MapGet("/forms/{formId:guid}/subscribers/export", async (
            Guid formId,
            IFormsRepository formsRepository,
            ISubscriptionProviderFactory subscriptionProviderFactory,
            HttpContext context,
            [FromQuery] string format = "csv"
        ) =>
        {
            if (format is not "csv" and not "json")
            {
                return ErrorResults.BadRequest("Invalid format", "Valid formats are: csv, json");
            }

            var form = await formsRepository.FindByIdAsync(formId);
            if (form == null)
            {
                return ErrorResults.NotFound();
            }

            if (form.Subscription is null)
            {
                return ErrorResults.BadRequest(
                    error: "This form does not support subscriptions",
                    cause: "Please set a subscription provider first"
                );
            }

            if (!subscriptionProviderFactory.TryCreate(form.Subscription.Provider, out var provider))
            {
                throw new Exception(
                    $"Invalid configuration exception, the form {formId} references a non-existing subscription provider: {form.Subscription.Provider}"
                );
            }

            var items = await provider.GetAllSubscribersAsync(formId);

            switch (format)
            {
                case "csv":
                    await ExportToCsv(context.Response, items);
                    break;
                case "json":
                    await ExportToJson(context.Response, items);
                    break;
            }
            
            return Results.Empty;
        });
    }

    private static async Task ExportToJson<T>(HttpResponse response, IEnumerable<T> data)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        await JsonSerializer.SerializeAsync(response.Body, data);
    }

    private static async Task ExportToCsv<T>(HttpResponse response, IEnumerable<T> data)
    {
        response.ContentType = MediaTypeNames.Text.Csv;
        await CsvSerializer.SerializeAsync(response.Body, data);
    }
}