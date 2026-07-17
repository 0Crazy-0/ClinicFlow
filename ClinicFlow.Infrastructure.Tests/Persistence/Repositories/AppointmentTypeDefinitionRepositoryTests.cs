using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class AppointmentTypeDefinitionRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly AppointmentTypeDefinitionRepository _sut = new(fixture.Context);
    private ApplicationDbContext Context => fixture.Context;

    public async ValueTask InitializeAsync()
    {
        await fixture.Respawner.ResetAsync(fixture.DbConnection);

        fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAppointmentTypeToContext()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();

        // Act
        await _sut.CreateAsync(appointmentType, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .AppointmentTypes.AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == appointmentType.Id,
                TestContext.Current.CancellationToken
            );

        dbResult.Should().BeEquivalentTo(appointmentType);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAppointmentType_WhenExistsAndActive()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType();

        Context.AppointmentTypes.Add(appointmentTypes);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType();

        appointmentTypes.Deactivate();

        Context.AppointmentTypes.Add(appointmentTypes);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeRequiredTemplates()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType();
        var template = ClinicalFormTemplate.Create("TEMP01", "Vitals Form", "Description", "{}");

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        appointmentTypes.AddRequiredTemplate(template);

        Context.AppointmentTypes.Add(appointmentTypes);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveAppointmentTypes()
    {
        // Arrange
        var active1 = CreateAppointmentType();
        var active2 = CreateAppointmentType();
        var inactive = CreateAppointmentType();

        inactive.Deactivate();

        Context.AppointmentTypes.AddRange(active1, active2, inactive);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetAllActiveAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([active1, active2]);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyMatchingActiveCategory()
    {
        // Arrange
        var firstConsult = CreateAppointmentType(category: AppointmentCategory.FirstConsultation);
        var followUp = CreateAppointmentType(category: AppointmentCategory.FollowUp);
        var inactiveFirst = CreateAppointmentType(category: AppointmentCategory.FirstConsultation);

        inactiveFirst.Deactivate();

        Context.AppointmentTypes.AddRange(firstConsult, followUp, inactiveFirst);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetByCategoryAsync(
            AppointmentCategory.FirstConsultation,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(firstConsult);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsEligible()
    {
        // Arrange
        var noRestriction = CreateAppointmentType(AgeEligibilityPolicy.NoRestriction);
        var kidOnly = CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));
        var adultOnly = CreateAppointmentType(AgeEligibilityPolicy.Create(18, 120, false));
        var inactive = CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));

        inactive.Deactivate();

        Context.AppointmentTypes.AddRange(noRestriction, kidOnly, adultOnly, inactive);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEligibleByAgeAsync(10, TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([noRestriction, kidOnly]);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsExactlyMinimumAge()
    {
        // Arrange

        var adultOnly = CreateAppointmentType(AgeEligibilityPolicy.Create(18, 120, false));

        Context.AppointmentTypes.Add(adultOnly);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEligibleByAgeAsync(18, TestContext.Current.CancellationToken);

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(adultOnly);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsExactlyMaximumAge()
    {
        // Arrange
        var kidOnly = CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));

        Context.AppointmentTypes.Add(kidOnly);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEligibleByAgeAsync(12, TestContext.Current.CancellationToken);

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(kidOnly);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType("Checkup");

        Context.AppointmentTypes.Add(appointmentTypes);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync("Checkup", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
    {
        // Arrange
        var inactive = CreateAppointmentType("Checkup");

        inactive.Deactivate();

        Context.AppointmentTypes.Add(inactive);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync("Checkup", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedAppointmentType()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType();

        appointmentTypes.Deactivate();

        Context.AppointmentTypes.Add(appointmentTypes);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
    {
        // Arrange
        // Duplicate names are prevented at the handler level (see DomainErrors.AppointmentType.NameAlreadyExists).
        // Two are seeded here only to test the repository's exclusion logic in isolation.
        var appointmentTypes1 = CreateAppointmentType("Name1");
        var appointmentTypes2 = CreateAppointmentType("Name1");

        Context.AppointmentTypes.AddRange(appointmentTypes1, appointmentTypes2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Name1",
            appointmentTypes1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
    {
        // Arrange
        var appointmentTypes = CreateAppointmentType();

        Context.AppointmentTypes.Add(appointmentTypes);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Name",
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    private static AppointmentTypeDefinition CreateAppointmentType(
        AgeEligibilityPolicy? agePolicy = null,
        AppointmentCategory category = AppointmentCategory.FirstConsultation
    ) =>
        AppointmentTypeDefinition.Create(
            category,
            "Name",
            "Description",
            EncounterDuration.FromMinutes(20),
            agePolicy ?? AgeEligibilityPolicy.NoRestriction
        );

    private static AppointmentTypeDefinition CreateAppointmentType(
        string name,
        AppointmentCategory category = AppointmentCategory.FirstConsultation
    ) =>
        AppointmentTypeDefinition.Create(
            category,
            name,
            "Description",
            EncounterDuration.FromMinutes(20),
            AgeEligibilityPolicy.NoRestriction
        );
}
