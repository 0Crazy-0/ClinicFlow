using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClinicFlow.Infrastructure.Tests.Persistence;

public class ApplicationDbContextModelTests
{
    private readonly ApplicationDbContext _sut;

    public ApplicationDbContextModelTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _sut = new ApplicationDbContext(options);
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureSequenceNumberValueGeneratedOnAdd_ForAllEntitiesInheritingBaseEntity()
    {
        // Arrange
        var entityTypes = _sut
            .Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
            .ToList();

        // Act & Assert
        entityTypes.Should().NotBeEmpty();

        foreach (var entityType in entityTypes)
        {
            var property = entityType.FindProperty(nameof(BaseEntity.SequenceNumber));

            property.Should().NotBeNull();
            property.ValueGenerated.Should().Be(ValueGenerated.OnAdd);
        }
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureVersionAsRowVersion_ForAllEntitiesInheritingBaseEntity()
    {
        // Arrange
        var entityTypes = _sut
            .Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType))
            .ToList();

        // Act & Assert
        entityTypes.Should().NotBeEmpty();

        foreach (var entityType in entityTypes)
        {
            var property = entityType.FindProperty(nameof(BaseEntity.Version));

            property.Should().NotBeNull();
            property.IsConcurrencyToken.Should().BeTrue();
            property.ValueGenerated.Should().Be(ValueGenerated.OnAddOrUpdate);
        }
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureQueryFilterWithParameterNameE_ForAllEntitiesInheritingSoftDeletableEntity()
    {
        // Arrange
        var entityTypes = _sut
            .Model.GetEntityTypes()
            .Where(e => typeof(SoftDeletableEntity).IsAssignableFrom(e.ClrType))
            .ToList();

        // Act & Assert
        entityTypes.Should().NotBeEmpty();

        foreach (var entityType in entityTypes)
        {
            var queryFilter = entityType.GetDeclaredQueryFilters().Single().Expression;

            queryFilter.Should().NotBeNull();
            queryFilter.Parameters.Should().ContainSingle().Which.Name.Should().Be("e");
            queryFilter.Body.ToString().Should().Be("(e.IsDeleted == False)");
        }
    }
}
