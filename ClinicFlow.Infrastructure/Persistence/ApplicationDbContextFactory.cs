using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicFlow.Infrastructure.Persistence;

/// <summary>
/// Temporary design-time factory for EF Core CLI tools (migrations).
/// </summary>
/// <remarks>
/// This is a temporary component for local migrations. It must be removed once the API layer
/// is implemented and provides its own dependency injection and configuration setup.
/// </remarks>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <inheritdoc />
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=ClinicFlowDb;Username=postgres;Password=postgres"
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
