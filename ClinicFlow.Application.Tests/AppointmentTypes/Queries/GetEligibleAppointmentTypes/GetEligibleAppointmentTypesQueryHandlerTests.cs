using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetEligibleAppointmentTypes;

public class GetEligibleAppointmentTypesQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetEligibleAppointmentTypesQueryHandler _sut;

    public GetEligibleAppointmentTypesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetEligibleAppointmentTypesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEligibleTypes_WhenTypesMatchAge()
    {
        // Arrange
        var adultType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Adult Checkup",
            "For adults",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, 65, false)
        );
        var template = ClinicalFormTemplate.Create(
            "BP_CHECK",
            "Blood Pressure",
            "Check blood pressure",
            """{"type": "object"}"""
        );
        adultType.AddRequiredTemplate(template);

        _repositoryMock
            .Setup(x => x.GetEligibleByAgeAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync([adultType]);

        var query = new GetEligibleAppointmentTypesQuery(30);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<AppointmentTypeDefinition> { adultType }.Select(
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
            x => x.GetEligibleByAgeAsync(30, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTypesMatchAge()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetEligibleByAgeAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetEligibleAppointmentTypesQuery(10);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(
            x => x.GetEligibleByAgeAsync(10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
