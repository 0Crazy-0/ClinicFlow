using Microsoft.Extensions.Configuration;

namespace ClinicFlow.Infrastructure.Persistence.Options;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string SsnHashKey { get; init; } = string.Empty;

    public static SecurityOptions FromConfiguration(IConfiguration configuration) =>
        configuration.GetSection(SectionName).Get<SecurityOptions>() ?? new();
}
