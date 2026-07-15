using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
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
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<AppointmentTypeDefinition> { type1 }.Select(
            appointmentType => new AppointmentTypeDto(
                appointmentType.Id,
                appointmentType.Category.ToString(),
                appointmentType.Name,
                appointmentType.Description,
                appointmentType.Duration.Minutes,
                appointmentType.AgePolicy.MinimumAge,
                appointmentType.AgePolicy.MaximumAge,
                appointmentType.AgePolicy.RequiresLegalGuardian,
                appointmentType.IsUnrestrictedBySpecialty,
                appointmentType.AllowedSpecialtyIds,
                [
                    .. appointmentType.RequiredTemplates.Select(t => new ClinicalFormTemplateDto(
                        t.Id,
                        t.Code,
                        t.Name,
                        t.Description,
                        t.JsonSchemaDefinition,
                        t.IsDeleted
                    )),
                ]
            )
        );

        result.Should().BeEquivalentTo(expectedDtos);

        _repositoryMock.Verify(
            x => x.GetByCategoryAsync(AppointmentCategory.Checkup, It.IsAny<CancellationToken>()),
            Times.Once
        );
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
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(
            x => x.GetByCategoryAsync(AppointmentCategory.Emergency, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
