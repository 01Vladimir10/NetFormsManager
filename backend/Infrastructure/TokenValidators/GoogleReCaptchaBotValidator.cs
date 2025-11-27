using System.Text.Json;
using NetFormsManager.Core.Services;

namespace NetFormsManager.Infrastructure.TokenValidators;

public class GoogleReCaptchaBotValidator(IHttpClientFactory httpClientFactory) : IBotValidatorProvider
{
    private const string GoogleRecaptchaVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    private class GoogleRecaptchaResponse
    {
        public bool Success { get; set; }
        public string? ChallengeTs { get; set; }
        public string? Hostname { get; set; }
        public string[]? ErrorCodes { get; set; }
    }

    public async Task<bool> ValidateAsync(string token, Dictionary<string, string> parameters, HttpContext httpContext)
    {
        if (!parameters.TryGetValue("Secret", out var secret))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var httpClient = httpClientFactory.CreateClient();
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

            var formData = new List<KeyValuePair<string, string>>
            {
                new("secret", secret),
                new("response", token)
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                formData.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
            }

            var content = new FormUrlEncodedContent(formData);

            var response = await httpClient.PostAsync(GoogleRecaptchaVerifyUrl, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GoogleRecaptchaResponse>(JsonSerializerOptions.Web);
            
            return result?.Success ?? false;
        }
        catch
        {
            // Log error if needed, but return false for any validation failures
            return false;
        }
    }

    private const string SecretParameterName = "secret";

    public static BotValidationProviderMetadata GetMetadata() => new(
        new Dictionary<string, BotValidationParameterInfo>
        {
            [SecretParameterName] = BotValidationParameterInfo.Required("The google reCaptcha site secret"),
        },
        parameters =>
        {
            if (parameters.TryGetValue(SecretParameterName, out var secret) && !string.IsNullOrWhiteSpace(secret))
                return [];
            return new Dictionary<string, List<string>>
            {
                [SecretParameterName] = [$"'{SecretParameterName}' is required and it must not be an empty string."]
            };
        }
    );
}