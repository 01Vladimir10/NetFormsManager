using System.Diagnostics.CodeAnalysis;

namespace NetMailGun.Core.Services;

public interface IBotValidatorProvider
{
    public Task<bool> ValidateAsync(string token, Dictionary<string, string> parameters, HttpContext httpContext);

    public static virtual BotValidationProviderMetadata GetMetadata() => new([], _ => []);
}

public delegate Dictionary<string, List<string>> BotValidatorParametersValidationFn(
    Dictionary<string, string> parameters);

public record BotValidationProviderMetadata(
    Dictionary<string, BotValidationParameterInfo> Parameters,
    BotValidatorParametersValidationFn ParametersValidator
);

public record BotValidationParameterInfo(string Description, bool IsRequired)
{
    public static BotValidationParameterInfo Required(string description) => new(description, true);
}

public interface IBotValidatorFactory
{
    public bool TryCreate(string provider, [NotNullWhen(true)] out IBotValidatorProvider? instance);
    public IEnumerable<string> EnumerateProviders();

    public bool TryGetMetadata(
        string provider,
        [NotNullWhen(true)] out BotValidationProviderMetadata? providerMetadata
    );
}