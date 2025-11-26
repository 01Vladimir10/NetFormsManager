using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NetMailGun.Api;
using NetMailGun.Api.Mappers;
using NetMailGun.Core.Model;
using NetMailGun.Core.Repositories;

namespace NetMailGun.Endpoints;

public static class FormsEndpoints
{
    public static void MapFormEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/forms", async (IFormsRepository formsRepository) =>
        {
            var forms = await formsRepository.GetAllAsync();
            return Results.Ok(forms.Select(x => x.ToDto()));
        });

        endpoints.MapGet("/forms/{formId:guid}", async (Guid formId, IFormsRepository formsRepository) =>
        {
            var form = await formsRepository.FindByIdAsync(formId);
            return form is null ? ErrorResults.NotFound() : Results.Ok(form.ToDto());
        });
        
        endpoints.MapPost("/forms",
            async (IFormsRepository formsRepository, IValidator<FormRequestDto> validator,
                [FromBody] FormRequestDto request) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ErrorResults.InvalidRequest(validationResult);
                }

                var form = FormEntity.FromDto(request, Guid.CreateVersion7());
                form.CreatedAt = DateTime.UtcNow;
                await formsRepository.CreateAsync(form);
                return Results.Ok(form.ToDto());
            }
        );
        endpoints.MapPut("/forms/{formId:guid}", async (IFormsRepository formsRepository,
                IValidator<FormRequestDto> validator,
                Guid formId,
                [FromBody] FormRequestDto request) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ErrorResults.InvalidRequest(validationResult);
                }

                var existingForm = await formsRepository.FindByIdAsync(formId);

                if (existingForm == null)
                {
                    return ErrorResults.NotFound();
                }

                var form = FormEntity.FromDto(request, formId, existingForm.CreatedAt);
                existingForm.Id = form.Id;
                existingForm.Name = form.Name;
                existingForm.Fields = form.Fields;
                existingForm.AllowedOrigins = form.AllowedOrigins;
                existingForm.BotValidator = form.BotValidator;
                existingForm.LastUpdatedAt = DateTime.UtcNow;
    
                await formsRepository.UpdateAsync(form);
                return Results.Ok(form.ToDto());
            }
        );
        endpoints.MapDelete("/forms/{formId:guid}", async (IFormsRepository formsRepository, Guid formId) =>
        {
            var existingForm = await formsRepository.FindByIdAsync(formId);

            if (existingForm == null)
            {
                return ErrorResults.NotFound();
            }

            await formsRepository.DeleteByIdAsync(formId);

            return Results.Ok(existingForm.ToDto());
        });
    }
}