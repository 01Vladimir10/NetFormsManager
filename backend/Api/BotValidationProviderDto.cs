using NetFormsManager.Core.Services;

namespace NetFormsManager.Api;

public record BotValidationProviderDto(string Name, IDictionary<string, BotValidationParameterInfo> Parameters);