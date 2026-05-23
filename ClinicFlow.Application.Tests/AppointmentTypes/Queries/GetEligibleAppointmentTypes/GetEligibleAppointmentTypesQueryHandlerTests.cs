using ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
            TimeSpan.FromMinutes(30),
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
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be(adultType.Name);
        result[0].MinimumAge.Should().Be(18);
        result[0].MaximumAge.Should().Be(65);
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
    public async Task Handle_ShouldReturnEmptyList_WhenNoTypesMatchAge()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetEligibleByAgeAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetEligibleAppointmentTypesQuery(10);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
