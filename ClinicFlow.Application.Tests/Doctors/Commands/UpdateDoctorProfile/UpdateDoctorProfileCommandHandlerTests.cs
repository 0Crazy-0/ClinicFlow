using ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Commands.UpdateDoctorProfile;

public class UpdateDoctorProfileCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateDoctorProfileCommandHandler _sut;

    public UpdateDoctorProfileCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new UpdateDoctorProfileCommandHandler(
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateProfile_WhenDoctorExists()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Updated biography with new certifications",
            5,
            "Dermatology B",
            5
        );

        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Original biography",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        doctor.Biography.Should().Be(command.Biography);
        doctor.ConsultationRoom.Number.Should().Be(command.ConsultationRoomNumber);
        doctor.ConsultationRoom.Name.Should().Be(command.ConsultationRoomName);
        doctor.ConsultationRoom.Floor.Should().Be(command.ConsultationRoomFloor);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDoctorDoesNotExist()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "New biography",
            5,
            "Room B",
            5
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
