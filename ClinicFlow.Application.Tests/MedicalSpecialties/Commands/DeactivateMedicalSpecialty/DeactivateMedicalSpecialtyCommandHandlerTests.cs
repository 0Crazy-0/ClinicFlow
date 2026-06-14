using AwesomeAssertions;
using ClinicFlow.Application.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;

public class DeactivateMedicalSpecialtyCommandHandlerTests
{
    private readonly Mock<IMedicalSpecialtyRepository> _medicalSpecialtyRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateMedicalSpecialtyCommandHandler _sut;

    public DeactivateMedicalSpecialtyCommandHandlerTests()
    {
        _medicalSpecialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeactivateMedicalSpecialtyCommandHandler(
            _medicalSpecialtyRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateSpecialty_WhenNoActiveDoctors()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create("Cardiology", "Heart specialty", 30, 24);
        var command = new DeactivateMedicalSpecialtyCommand(specialty.Id);

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SpecialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);

        _doctorRepositoryMock
            .Setup(x => x.HasActiveBySpecialtyIdAsync(specialty.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        specialty.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new DeactivateMedicalSpecialtyCommand(Guid.NewGuid());

        _medicalSpecialtyRepositoryMock
            .Setup(x => x.GetByIdAsync(command.SpecialtyId, It.IsAny<CancellationToken>()))
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
