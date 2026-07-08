using System.Text.Json.Nodes;
using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class ClinicalFormTemplateRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly ClinicalFormTemplateRepository _sut = new(fixture.Context);
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
    public async Task GetByIdAsync_ShouldReturnTemplate_WhenExistsAndActive()
    {
        // Arrange
        var template = CreateTemplate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(template.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(template.Id);
        result.Code.Should().Be(template.Code);
        result.Name.Should().Be(template.Name);
        result.Description.Should().Be(template.Description);
        result.IsDeleted.Should().BeFalse();

        AssertJsonEquivalent(result.JsonSchemaDefinition, template.JsonSchemaDefinition);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var template = CreateTemplate();

        template.Deactivate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(template.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnTemplate_WhenExistsAndActive()
    {
        // Arrange
        var template = CreateTemplate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByCodeAsync(
            template.Code,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(template.Code);
        result.Id.Should().Be(template.Id);
        result.Name.Should().Be(template.Name);
        result.Description.Should().Be(template.Description);
        result.IsDeleted.Should().BeFalse();

        AssertJsonEquivalent(result.JsonSchemaDefinition, template.JsonSchemaDefinition);
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentCode = "NONEXISTENT";

        // Act
        var result = await _sut.GetByCodeAsync(
            nonExistentCode,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var template = CreateTemplate();

        template.Deactivate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByCodeAsync(
            template.Code,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByCodeAsync_ShouldReturnTrue_WhenActiveExistsWithCode()
    {
        // Arrange
        var template = CreateTemplate(code: "TEMP01");

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByCodeAsync("TEMP01", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCodeAsync_ShouldReturnFalse_WhenNoActiveWithCode()
    {
        // Arrange
        var template = CreateTemplate(code: "TEMP01");

        template.Deactivate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByCodeAsync("TEMP01", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
    {
        // Arrange
        var template = CreateTemplate(name: "Vitals Form");

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync(
            "Vitals Form",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
    {
        // Arrange
        var template = CreateTemplate(name: "Vitals Form");

        template.Deactivate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync(
            "Vitals Form",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedTemplate()
    {
        // Arrange
        var template = CreateTemplate(code: "pene grande 123");

        template.Deactivate();

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            template.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be(template.Code);
        result.Id.Should().Be(template.Id);
        result.Name.Should().Be(template.Name);
        result.Description.Should().Be(template.Description);
        result.IsDeleted.Should().BeTrue();

        AssertJsonEquivalent(result.JsonSchemaDefinition, template.JsonSchemaDefinition);
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange & Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
    {
        // Arrange
        // Duplicate names are prevented at the handler level (see DomainErrors.ClinicalFormTemplate.NameAlreadyExists).
        // Two are seeded here only to test the repository's exclusion logic in isolation.
        var template1 = CreateTemplate(code: "TEMP01", name: "FormName");
        var template2 = CreateTemplate(code: "TEMP02", name: "FormName");

        Context.ClinicalFormTemplates.AddRange(template1, template2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "FormName",
            template1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
    {
        // Arrange
        var template = CreateTemplate(name: "FormName");

        Context.ClinicalFormTemplates.Add(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "FormName",
            template.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveTemplates()
    {
        // Arrange
        var active1 = CreateTemplate(code: "TEMP01");
        var active2 = CreateTemplate(code: "TEMP02");
        var inactive = CreateTemplate(code: "TEMP03");

        inactive.Deactivate();

        Context.ClinicalFormTemplates.AddRange(active1, active2, inactive);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetAllActiveAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([active1, active2]);
    }

    [Fact]
    public async Task GetAllIncludingDeletedAsync_ShouldReturnAllActiveAndDeletedTemplates()
    {
        // Arrange
        var active1 = CreateTemplate(code: "TEMP01");
        var active2 = CreateTemplate(code: "TEMP02");
        var inactive = CreateTemplate(code: "TEMP03");

        inactive.Deactivate();

        Context.ClinicalFormTemplates.AddRange(active1, active2, inactive);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetAllIncludingDeletedAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([active1, active2, inactive]);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddTemplateToContext()
    {
        // Arrange
        var template = CreateTemplate();

        // Act
        await _sut.CreateAsync(template, TestContext.Current.CancellationToken);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .ClinicalFormTemplates.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == template.Id, TestContext.Current.CancellationToken);

        dbResult.Should().NotBeNull();
        dbResult.Code.Should().Be(template.Code);
        dbResult.Name.Should().Be(template.Name);
        dbResult.Description.Should().Be(template.Description);
        dbResult.IsDeleted.Should().BeFalse();

        AssertJsonEquivalent(dbResult.JsonSchemaDefinition, template.JsonSchemaDefinition);
    }

    private static ClinicalFormTemplate CreateTemplate(
        string code = "TEMP99",
        string name = "Default Template",
        string description = "Default Description",
        string jsonSchema = """{"type": "object"}"""
    ) => ClinicalFormTemplate.Create(code, name, description, jsonSchema);

    private static void AssertJsonEquivalent(string actualJson, string expectedJson)
    {
        var actual = JsonNode.Parse(actualJson);
        var expected = JsonNode.Parse(expectedJson);

        JsonNode.DeepEquals(actual, expected).Should().BeTrue();
    }
}
