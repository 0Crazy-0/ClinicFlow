using Microsoft.Extensions.Configuration;

namespace ClinicFlow.Infrastructure.Time;

public sealed class ClinicOptions
{
    public const string SectionName = "Clinic";

    public string TimeZoneId { get; init; } = "America/Los_Angeles";

    public static ClinicOptions FromConfiguration(IConfiguration configuration) =>
        configuration.GetSection(SectionName).Get<ClinicOptions>() ?? new ClinicOptions();
}
