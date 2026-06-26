using AwesomeAssertions;
using ClinicFlow.Application.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;
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
        result.Should().HaveCount(2);
        result[0].Name.Should().Be(type1.Name);
        result[0].IsUnrestrictedBySpecialty.Should().BeTrue();
        result[0].AllowedSpecialtyIds.Should().BeEmpty();
        result[0].RequiredTemplates.Should().ContainSingle();
        result[1].Name.Should().Be(type2.Name);
        result[1].IsUnrestrictedBySpecialty.Should().BeTrue();
        result[1].AllowedSpecialtyIds.Should().BeEmpty();
        result[1].RequiredTemplates.Should().BeEmpty();

        var mappedTemplate = result[0].RequiredTemplates.First();
        mappedTemplate.Id.Should().Be(template.Id);
        mappedTemplate.Code.Should().Be(template.Code);
        mappedTemplate.Name.Should().Be(template.Name);
        mappedTemplate.Description.Should().Be(template.Description);
        mappedTemplate.JsonSchemaDefinition.Should().Be(template.JsonSchemaDefinition);
        mappedTemplate.IsDeleted.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
