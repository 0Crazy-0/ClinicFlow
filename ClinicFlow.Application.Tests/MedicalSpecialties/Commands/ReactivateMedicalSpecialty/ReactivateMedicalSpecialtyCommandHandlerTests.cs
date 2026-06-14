using AwesomeAssertions;
using ClinicFlow.Application.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;

public class ReactivateMedicalSpecialtyCommandHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _medicalSpecialtyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReactivateMedicalSpecialtyCommandHandler _sut;

    public ReactivateMedicalSpecialtyCommandHandlerTests()
    {
        _medicalSpecialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ReactivateMedicalSpecialtyCommandHandler(
            _medicalSpecialtyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReactivateSpecialty_WhenInactive()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create("Cardiology", "Heart specialty", 30, 24);
        specialty.Deactivate(false);

        var command = new ReactivateMedicalSpecialtyCommand(specialty.Id);

        _medicalSpecialtyRepositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.SpecialtyId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(specialty);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        specialty.IsDeleted.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new ReactivateMedicalSpecialtyCommand(Guid.NewGuid());

        _medicalSpecialtyRepositoryMock
            .Setup(x =>
                x.GetByIdIncludingDeletedAsync(command.SpecialtyId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((MedicalSpecialty?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
