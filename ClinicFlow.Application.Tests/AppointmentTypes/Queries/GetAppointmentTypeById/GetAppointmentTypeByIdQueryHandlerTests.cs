using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypeById;

public class GetAppointmentTypeByIdQueryHandlerTests
{
    private readonly Mock<IAppointmentTypeDefinitionRepository> _repositoryMock;
    private readonly GetAppointmentTypeByIdQueryHandler _sut;

    public GetAppointmentTypeByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _sut = new GetAppointmentTypeByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenEntityExists()
    {
        // Arrange
        var entity = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            EncounterDuration.FromMinutes(30)
        );

        var template = ClinicalFormTemplate.Create(
            "BP_CHECK",
            "Blood Pressure",
            "Check blood pressure",
            """{"type": "object"}"""
        );
        entity.AddRequiredTemplate(template);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var query = new GetAppointmentTypeByIdQuery(entity.Id);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new AppointmentTypeDto(
            entity.Id,
            entity.Category.ToString(),
            entity.Name,
            entity.Description,
            entity.Duration.Minutes,
            entity.AgePolicy.MinimumAge,
            entity.AgePolicy.MaximumAge,
            entity.AgePolicy.RequiresLegalGuardian,
            entity.IsUnrestrictedBySpecialty,
            entity.AllowedSpecialtyIds,
            [
                .. entity.RequiredTemplates.Select(t => new ClinicalFormTemplateDto(
                    t.Id,
                    t.Code,
                    t.Name,
                    t.Description,
                    t.JsonSchemaDefinition,
                    t.IsDeleted
                )),
            ]
        );

        result.Should().BeEquivalentTo(expectedDto);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        var query = new GetAppointmentTypeByIdQuery(id);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _repositoryMock.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
