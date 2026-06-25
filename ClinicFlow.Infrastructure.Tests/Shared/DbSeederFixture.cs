using System.Data.Common;
using ClinicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Testcontainers.PostgreSql;

namespace ClinicFlow.Infrastructure.Tests.Shared;

public class DbSeederFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder(
        "postgres:17-alpine"
    ).Build();

    public ApplicationDbContext Context { get; private set; } = null!;
    public DbConnection DbConnection { get; private set; } = null!;
    public Respawner Respawner { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        Context = new ApplicationDbContext(options);

        // Database exists but has no tables yet — EnsureCreatedAsync creates the schema from EF Core model.
        await Context.Database.EnsureCreatedAsync();

        DbConnection = Context.Database.GetDbConnection();

        await DbConnection.OpenAsync();

        Respawner = await Respawner.CreateAsync(
            DbConnection,
            new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["public"] }
        );
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
