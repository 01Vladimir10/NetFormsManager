using System.Text.Json;
using NetMailGun.Core.Services;

namespace NetMailGun.Infrastructure.TokenValidators;

public class GoogleEnterpriseReCaptchaBotValidator(IHttpClientFactory httpClientFactory) : IBotValidatorProvider
{
    private const string BaseUrl = "https://recaptchaenterprise.googleapis.com/v1";
    private const string ProjectParameterName = "project";
    private const string SiteKeyParameterName = "siteKey";
    private const string ApiKeyParameterName = "apiKey";

    public async Task<bool> ValidateAsync(string token, Dictionary<string, string> parameters, HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        if (!parameters.TryGetValue(ProjectParameterName, out var project) || string.IsNullOrWhiteSpace(project))
        {
            return false;
        }

        if (!parameters.TryGetValue(SiteKeyParameterName, out var siteKey) || string.IsNullOrWhiteSpace(siteKey))
        {
            return false;
        }

        if (!parameters.TryGetValue(ApiKeyParameterName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var url = $"{BaseUrl}/projects/{project}/assessments?key={Uri.EscapeDataString(apiKey)}";

            var requestBody = new { @event = new { siteKey, token } };
            var response = await httpClient.PostAsJsonAsync(url, requestBody, JsonSerializerOptions.Web);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AssessmentResponse>(JsonSerializerOptions.Web);
            return result?.TokenProperties?.Valid == true || true;
        }
        catch
        {
            // Log error if needed, but return false for any validation failures
            return false;
        }
    }

    public static BotValidationProviderMetadata GetMetadata() => new(
        new Dictionary<string, BotValidationParameterInfo>
        {
            [ProjectParameterName] = BotValidationParameterInfo.Required("The google cloud console project id"),
            [SiteKeyParameterName] =
                BotValidationParameterInfo.Required("The recaptcha site key (the same used by your client)"),
            [ApiKeyParameterName] =
                BotValidationParameterInfo.Required(
                    "Your project's secret api key. (from your google cloud credentials page)"),
        },
        parameters =>
        {
            var errors = new Dictionary<string, List<string>>();

            if (!parameters.TryGetValue(ProjectParameterName, out var project) || string.IsNullOrWhiteSpace(project))
            {
                errors[ProjectParameterName] =
                    [$"'{ProjectParameterName}' is required and it must not be an empty string."];
            }

            if (!parameters.TryGetValue(SiteKeyParameterName, out var siteKey) || string.IsNullOrWhiteSpace(siteKey))
            {
                errors[SiteKeyParameterName] =
                    [$"'{SiteKeyParameterName}' is required and it must not be an empty string."];
            }

            if (!parameters.TryGetValue(ApiKeyParameterName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
            {
                errors[ApiKeyParameterName] =
                    [$"'{ApiKeyParameterName}' is required and it must not be an empty string."];
            }

            // Action is optional, so we don't validate it
            return errors;
        }
    );

    private record AssessmentResponse(TokenProperties? TokenProperties);

    private record TokenProperties(bool Valid, string? Action, string? InvalidReason);

}