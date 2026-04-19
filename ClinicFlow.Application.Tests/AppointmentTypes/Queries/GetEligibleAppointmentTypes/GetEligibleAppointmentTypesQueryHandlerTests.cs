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

        _repositoryMock
            .Setup(x => x.GetEligibleByAgeAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync([adultType]);

        var query = new GetEligibleAppointmentTypesQuery(30);
        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Adult Checkup");
        result[0].MinimumAge.Should().Be(18);
        result[0].MaximumAge.Should().Be(65);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTypesMatchAge()
    {
        // Arrange
        var teenType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Teen Checkup",
            "For teens",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(11, 17, false)
        );

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
