using AwesomeAssertions;
using ClinicFlow.Application.MedicalSpecialties.Queries.GetMedicalSpecialtyById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Queries.GetMedicalSpecialtyById;

public class GetMedicalSpecialtyByIdQueryHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _repositoryMock;
    private readonly GetMedicalSpecialtyByIdQueryHandler _sut;

    public GetMedicalSpecialtyByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _sut = new GetMedicalSpecialtyByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenSpecialtyExists()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create(
            "Cardiology",
            "Heart and cardiovascular system",
            45,
            24
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(specialty.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);

        var query = new GetMedicalSpecialtyByIdQuery(specialty.Id);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(specialty.Id);
        result.Name.Should().Be(specialty.Name);
        result.Description.Should().Be(specialty.Description);
        result.TypicalDurationMinutes.Should().Be(specialty.TypicalDuration.Minutes);
        result.MinCancellationHours.Should().Be(specialty.CancellationPolicy.Hours);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(specialty.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSpecialtyDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalSpecialty?)null);

        var query = new GetMedicalSpecialtyByIdQuery(id);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));

        _repositoryMock.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
