using NetMailGun.Core.Services;

namespace NetMailGun.Api;

public record BotValidationProviderDto(string Name, IDictionary<string, BotValidationParameterInfo> Parameters);