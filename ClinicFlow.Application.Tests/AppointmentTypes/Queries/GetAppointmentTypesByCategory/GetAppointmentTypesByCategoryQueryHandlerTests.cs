using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public class GetAppointmentTypesByCategoryQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAppointmentTypesByCategoryQueryHandler _sut;

    public GetAppointmentTypesByCategoryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAppointmentTypesByCategoryQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMatchingTypes_WhenCategoryHasTypes()
    {
        // Arrange
        var type1 = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine",
            EncounterDuration.FromMinutes(30)
        );
        var template = ClinicalFormTemplate.Create(
            "BP_CHECK",
            "Blood Pressure",
            "Check blood pressure",
            """{"type": "object"}"""
        );
        type1.AddRequiredTemplate(template);

        _repositoryMock
            .Setup(x =>
                x.GetByCategoryAsync(AppointmentCategory.Checkup, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([type1]);

        var query = new GetAppointmentTypesByCategoryQuery(AppointmentCategory.Checkup);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Category.Should().Be(nameof(AppointmentCategory.Checkup));
        result[0].IsUnrestrictedBySpecialty.Should().BeTrue();
        result[0].AllowedSpecialtyIds.Should().BeEmpty();
        result[0].RequiredTemplates.Should().ContainSingle();

        var mappedTemplate = result[0].RequiredTemplates.First();
        mappedTemplate.Id.Should().Be(template.Id);
        mappedTemplate.Code.Should().Be(template.Code);
        mappedTemplate.Name.Should().Be(template.Name);
        mappedTemplate.Description.Should().Be(template.Description);
        mappedTemplate.JsonSchemaDefinition.Should().Be(template.JsonSchemaDefinition);
        mappedTemplate.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenCategoryHasNoTypes()
    {
        // Arrange
        _repositoryMock
            .Setup(x =>
                x.GetByCategoryAsync(AppointmentCategory.Emergency, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        var query = new GetAppointmentTypesByCategoryQuery(AppointmentCategory.Emergency);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
