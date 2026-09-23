using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;

public class GetAppointmentTypesByPurposeQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAppointmentTypesByPurposeQueryHandler _sut;

    public GetAppointmentTypesByPurposeQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAppointmentTypesByPurposeQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMatchingTypes_WhenPurposeHasTypes()
    {
        // Arrange
        var type1 = AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
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
                x.GetByPurposeAsync(AppointmentPurpose.Checkup, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([type1]);

        var query = new GetAppointmentTypesByPurposeQuery(AppointmentPurpose.Checkup);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<AppointmentTypeDefinition> { type1 }.Select(
            appointmentType => new AppointmentTypeDto(
                appointmentType.Id,
                appointmentType.Category.ToString(),
                appointmentType.Purpose.ToString(),
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
            x => x.GetByPurposeAsync(AppointmentPurpose.Checkup, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenPurposeHasNoTypes()
    {
        // Arrange
        _repositoryMock
            .Setup(x =>
                x.GetByPurposeAsync(AppointmentPurpose.Emergency, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        var query = new GetAppointmentTypesByPurposeQuery(AppointmentPurpose.Emergency);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(
            x => x.GetByPurposeAsync(AppointmentPurpose.Emergency, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
