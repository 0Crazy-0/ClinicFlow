using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;

public class GetAllActiveAppointmentTypesQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAllActiveAppointmentTypesQueryHandler _sut;

    public GetAllActiveAppointmentTypesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAllActiveAppointmentTypesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveTypes_WhenTypesExist()
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

        var type2 = AppointmentTypeDefinition.Create(
            AppointmentCategory.FollowUp,
            "Follow Up",
            "Return visit",
            EncounterDuration.FromMinutes(20)
        );

        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([type1, type2]);

        var query = new GetAllActiveAppointmentTypesQuery();

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<AppointmentTypeDefinition> { type1, type2 }.Select(
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

        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTypesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetAllActiveAppointmentTypesQuery(),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
