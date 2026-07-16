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
        var template = await CreateTemplate();

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
        var template = await CreateTemplate();

        template.Deactivate();

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
        var template = await CreateTemplate();

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
        var template = await CreateTemplate();

        template.Deactivate();

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
        await CreateTemplate(code: "TEMP01");

        // Act
        var result = await _sut.ExistsByCodeAsync("TEMP01", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCodeAsync_ShouldReturnFalse_WhenNoActiveWithCode()
    {
        // Arrange
        var template = await CreateTemplate(code: "TEMP01");

        template.Deactivate();

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
        await CreateTemplate(name: "Vitals Form");

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
        var template = await CreateTemplate(name: "Vitals Form");

        template.Deactivate();

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
        var template = await CreateTemplate();

        template.Deactivate();

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
            Guid.CreateVersion7(),
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
        var template1 = await CreateTemplate(code: "TEMP01", name: "FormName");

        await CreateTemplate(code: "TEMP02", name: "FormName");

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
        var template = await CreateTemplate(name: "FormName");

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
        var active1 = await CreateTemplate(code: "TEMP01");
        var active2 = await CreateTemplate(code: "TEMP02");
        var inactive = await CreateTemplate(code: "TEMP03");

        inactive.Deactivate();

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
        var active1 = await CreateTemplate(code: "TEMP01");
        var active2 = await CreateTemplate(code: "TEMP02");
        var inactive = await CreateTemplate(code: "TEMP03");

        inactive.Deactivate();

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
        var template = ClinicalFormTemplate.Create(
            "TEMP99",
            "Default Template",
            "Default Description",
            """{"type": "object"}"""
        );

        // Act
        await _sut.CreateAsync(template, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .ClinicalFormTemplates.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == template.Id, TestContext.Current.CancellationToken);

        dbResult
            .Should()
            .BeEquivalentTo(template, options => options.Excluding(x => x.JsonSchemaDefinition));

        AssertJsonEquivalent(dbResult.JsonSchemaDefinition, template.JsonSchemaDefinition);
    }

    private async Task<ClinicalFormTemplate> CreateTemplate(
        string code = "TEMP99",
        string name = "Default Template",
        string description = "Default Description",
        string jsonSchema = """{"type": "object"}"""
    )
    {
        var template = ClinicalFormTemplate.Create(code, name, description, jsonSchema);

        Context.ClinicalFormTemplates.Add(template);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return template;
    }

    private static void AssertJsonEquivalent(string actualJson, string expectedJson)
    {
        var actual = JsonNode.Parse(actualJson);
        var expected = JsonNode.Parse(expectedJson);

        JsonNode.DeepEquals(actual, expected).Should().BeTrue();
    }
}
