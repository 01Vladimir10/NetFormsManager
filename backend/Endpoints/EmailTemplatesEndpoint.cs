using System.Net.Mime;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetFormsManager.Api;
using NetFormsManager.Api.Mappers;
using NetFormsManager.Core.Repositories;

namespace NetFormsManager.Endpoints;

public static class EmailTemplatesEndpoint
{
    public static void MapEmailTemplateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/forms/{formId:guid}/templates",
            async (Guid formId, IEmailTemplatesRepository emailTemplatesRepository) =>
            {
                var templates = await emailTemplatesRepository.GetByFormIdAsync(formId);
                return Results.Ok(templates.Select(x => x.ToDto()));
            });

        endpoints.MapGet("/forms/{formId:guid}/templates/{templateId:guid}",
            async (Guid formId, Guid templateId, IEmailTemplatesRepository emailTemplatesRepository) =>
            {
                var template = await emailTemplatesRepository.FindByIdAsync(formId, templateId);
                return template is null ? ErrorResults.NotFound() : Results.Ok(template.ToDto());
            });

        endpoints.MapPost("/forms/{formId:guid}/templates",
            async (IEmailTemplatesRepository emailTemplatesRepository, IFormsRepository formsRepository,
                IValidator<EmailTemplateRequestDto> validator,
                Guid formId,
                [FromBody] EmailTemplateRequestDto request) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ErrorResults.InvalidRequest(validationResult);
                }

                if (!await formsRepository.ExistsAsync(formId))
                {
                    return ErrorResults.NotFound();
                }

                var template = EmailTemplateMappers.FromDto(request, formId, Guid.CreateVersion7());
                await emailTemplatesRepository.CreateAsync(template);
                return Results.Ok(template.ToDto());
            }
        );

        endpoints.MapPut("/forms/{formId:guid}/templates/{templateId:guid}", async (
                IEmailTemplatesRepository emailTemplatesRepository,
                IValidator<EmailTemplateRequestDto> validator,
                Guid formId,
                Guid templateId,
                [FromBody] EmailTemplateRequestDto request) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ErrorResults.InvalidRequest(validationResult);
                }

                var existingTemplate = await emailTemplatesRepository.FindByIdAsync(formId, templateId);

                if (existingTemplate == null)
                {
                    return ErrorResults.NotFound();
                }

                var template = EmailTemplateMappers.FromDto(request, formId, templateId);
                existingTemplate.FormId = template.FormId;
                existingTemplate.IsEnabled = template.IsEnabled;
                existingTemplate.FromName = template.FromName;
                existingTemplate.SubjectTemplate = template.SubjectTemplate;
                existingTemplate.Body = template.Body;
                existingTemplate.ReplyTo = template.ReplyTo;
                existingTemplate.To = template.To;
                existingTemplate.Bcc = template.Bcc;
                existingTemplate.Cc = template.Cc;

                await emailTemplatesRepository.UpdateAsync(existingTemplate);
                return Results.Ok(existingTemplate.ToDto());
            }
        );

        endpoints.MapDelete("/forms/{formId:guid}/templates/{templateId:guid}",
            async (IEmailTemplatesRepository emailTemplatesRepository, Guid formId, Guid templateId) =>
            {
                var existingTemplate = await emailTemplatesRepository.FindByIdAsync(formId, templateId);

                if (existingTemplate == null)
                {
                    return ErrorResults.NotFound();
                }

                await emailTemplatesRepository.DeleteByIdAsync(formId, templateId);

                return Results.Ok(existingTemplate.ToDto());
            });

        endpoints.MapPut("/forms/{formId:guid}/templates/{templateId:guid}/enable", async (
            IEmailTemplatesRepository emailTemplatesRepository,
            Guid formId,
            Guid templateId) =>
        {
            var existingTemplate = await emailTemplatesRepository.FindByIdAsync(formId, templateId);

            if (existingTemplate == null)
            {
                return ErrorResults.NotFound();
            }

            existingTemplate.IsEnabled = true;
            await emailTemplatesRepository.UpdateAsync(existingTemplate);
            return Results.Ok(existingTemplate.ToDto());
        });

        endpoints.MapPut("/forms/{formId:guid}/templates/{templateId:guid}/disable", async (
            IEmailTemplatesRepository emailTemplatesRepository,
            Guid formId,
            Guid templateId) =>
        {
            var existingTemplate = await emailTemplatesRepository.FindByIdAsync(formId, templateId);

            if (existingTemplate == null)
            {
                return ErrorResults.NotFound();
            }

            existingTemplate.IsEnabled = false;
            await emailTemplatesRepository.UpdateAsync(existingTemplate);
            return Results.Ok(existingTemplate.ToDto());
        });

        endpoints.MapPut("/forms/{formId:guid}/templates/{templateId:guid}/body",
            async ([FromRoute] Guid formId, [FromRoute] Guid templateId, HttpRequest request,
                IEmailTemplatesRepository emailTemplatesRepository) =>
            {
                var existingTemplate = await emailTemplatesRepository.FindByIdAsync(formId, templateId);
                if (existingTemplate == null)
                {
                    return ErrorResults.NotFound();
                }

                if (request.ContentType != MediaTypeNames.Text.Plain)
                {
                    return ErrorResults.BadRequest("Unsupported media type",
                        "The content type of this request must be plain text");
                }

                using var reader = new StreamReader(request.Body);
                var template = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(template))
                {
                    return ErrorResults.BadRequest("Invalid template", "The content cannot be an empty string");
                }

                existingTemplate.Body = template;
                await emailTemplatesRepository.UpdateAsync(existingTemplate);
                return Results.Ok(existingTemplate.ToDto());
            })
            .Produces(StatusCodes.Status204NoContent);
    }
}