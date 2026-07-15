using AwesomeAssertions;
using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
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
        var expectedDto = new MedicalSpecialtyDto(
            specialty.Id,
            specialty.Name,
            specialty.Description,
            specialty.TypicalDuration.Minutes,
            specialty.CancellationPolicy.Hours,
            specialty.IsDeleted
        );

        result.Should().BeEquivalentTo(expectedDto);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(specialty.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSpecialtyDoesNotExist()
    {
        // Arrange
        var id = Guid.CreateVersion7();
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
