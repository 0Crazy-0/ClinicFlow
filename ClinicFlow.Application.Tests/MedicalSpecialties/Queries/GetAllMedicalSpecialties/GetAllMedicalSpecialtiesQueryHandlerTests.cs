using AwesomeAssertions;
using ClinicFlow.Application.MedicalSpecialties.Queries.GetAllMedicalSpecialties;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Queries.GetAllMedicalSpecialties;

public class GetAllMedicalSpecialtiesQueryHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _repositoryMock;
    private readonly GetAllMedicalSpecialtiesQueryHandler _sut;

    public GetAllMedicalSpecialtiesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _sut = new GetAllMedicalSpecialtiesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllSpecialtiesIncludingInactive_WhenSpecialtiesExist()
    {
        // Arrange
        var activeSpecialty = MedicalSpecialty.Create("Cardiology", "Heart care", 45, 24);
        var inactiveSpecialty = MedicalSpecialty.Create("Deprecated Specialty", "Old", 30, 12);
        inactiveSpecialty.Deactivate(false);

        _repositoryMock
            .Setup(x => x.GetAllIncludingDeletedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([activeSpecialty, inactiveSpecialty]);

        var query = new GetAllMedicalSpecialtiesQuery();

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsDeleted.Should().BeFalse();
        result[1].IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSpecialtiesExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllIncludingDeletedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(
            new GetAllMedicalSpecialtiesQuery(),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
