using ClinicFlow.Application.Doctors.Commands.SuspendDoctorProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.Commands.SuspendDoctorProfile;

public class SuspendDoctorProfileCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SuspendDoctorProfileCommandHandler _sut;

    public SuspendDoctorProfileCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new SuspendDoctorProfileCommandHandler(
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSuspendDoctor_WhenValidCommand()
    {
        // Arrange
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Biography",
            ConsultationRoom.Create(1, "Room", 1)
        );

        var command = new SuspendDoctorProfileCommand(doctor.Id);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        doctor.IsDeleted.Should().BeTrue();
        doctor.DomainEvents.OfType<DoctorSuspendedEvent>().Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new SuspendDoctorProfileCommand(Guid.NewGuid());

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
