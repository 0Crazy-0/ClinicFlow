using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
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
            TimeSpan.FromMinutes(30)
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
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);
        result.Category.Should().Be(nameof(AppointmentCategory.Checkup));
        result.Name.Should().Be(entity.Name);
        result.Description.Should().Be(entity.Description);
        result.DurationMinutes.Should().Be(entity.DurationMinutes);
        result.IsUnrestrictedBySpecialty.Should().Be(entity.IsUnrestrictedBySpecialty);
        result.AllowedSpecialtyIds.Should().BeEquivalentTo(entity.AllowedSpecialtyIds);

        result.RequiredTemplates.Should().ContainSingle();
        var mappedTemplate = result.RequiredTemplates.First();
        mappedTemplate.Id.Should().Be(template.Id);
        mappedTemplate.Code.Should().Be(template.Code);
        mappedTemplate.Name.Should().Be(template.Name);
        mappedTemplate.Description.Should().Be(template.Description);
        mappedTemplate.JsonSchemaDefinition.Should().Be(template.JsonSchemaDefinition);
        mappedTemplate.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEntityDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        var query = new GetAppointmentTypeByIdQuery(id);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }
}
