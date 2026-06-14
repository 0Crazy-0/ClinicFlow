using AwesomeAssertions;
using ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Commands.ReactivateDoctorProfile;

public class ReactivateDoctorProfileCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ReactivateDoctorProfileCommandHandler _sut;

    public ReactivateDoctorProfileCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ReactivateDoctorProfileCommandHandler(
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReactivateDoctor_WhenValidCommand()
    {
        // Arrange
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Old biography",
            ConsultationRoom.Create(1, "Old Room", 1)
        );

        doctor.Suspend();

        var command = new ReactivateDoctorProfileCommand(
            doctor.Id,
            "Biography",
            5,
            "Pediatrics",
            2
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        doctor.IsDeleted.Should().BeFalse();
        doctor.FullName.Should().Be(doctor.FullName);
        doctor.Biography.Should().Be(command.Biography);
        doctor.ConsultationRoom.Number.Should().Be(command.ConsultationRoomNumber);
        doctor.ConsultationRoom.Name.Should().Be(command.ConsultationRoomName);
        doctor.ConsultationRoom.Floor.Should().Be(command.ConsultationRoomFloor);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(Guid.NewGuid(), "Biography", 1, "Room", 1);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
