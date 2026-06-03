using Microsoft.Extensions.Configuration;

namespace ClinicFlow.Infrastructure.Persistence.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    private const string MissingConfigMessage =
        "Database configuration is missing. Ensure 'Database' section exists in appsettings.";

    public string ConnectionString { get; init; } = string.Empty;
    public bool SeedOnStartup { get; init; }

    public static DatabaseOptions FromConfiguration(IConfiguration configuration) =>
        configuration.GetSection(SectionName).Get<DatabaseOptions>()
        ?? throw new InvalidOperationException(MissingConfigMessage);
}
