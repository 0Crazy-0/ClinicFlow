using ClinicFlow.Application.MedicalSpecialties.Queries.GetActiveMedicalSpecialties;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Queries.GetActiveMedicalSpecialties;

public class GetActiveMedicalSpecialtiesQueryHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _repositoryMock;
    private readonly GetActiveMedicalSpecialtiesQueryHandler _sut;

    public GetActiveMedicalSpecialtiesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _sut = new GetActiveMedicalSpecialtiesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveSpecialties_WhenSpecialtiesExist()
    {
        // Arrange
        var specialty1 = MedicalSpecialty.Create("Cardiology", "Heart care", 45, 24);
        var specialty2 = MedicalSpecialty.Create("Dermatology", "Skin care", 30, 12);

        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([specialty1, specialty2]);

        var query = new GetActiveMedicalSpecialtiesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be(specialty1.Name);
        result[1].Name.Should().Be(specialty2.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSpecialtiesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetActiveMedicalSpecialtiesQuery(),
            CancellationToken.None
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
