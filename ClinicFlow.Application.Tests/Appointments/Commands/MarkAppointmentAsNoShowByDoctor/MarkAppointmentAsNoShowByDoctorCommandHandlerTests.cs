using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;

public class MarkAppointmentAsNoShowByDoctorCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkAppointmentAsNoShowByDoctorCommandHandler _sut;

    public MarkAppointmentAsNoShowByDoctorCommandHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new MarkAppointmentAsNoShowByDoctorCommandHandler(
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAsNoShow_WhenDoctorIdMatches()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(command.AppointmentId, doctorId);
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _appointmentRepositoryMock.Verify(
            x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenDoctorIdDoesNotMatchAppointment()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());
        var anotherDoctorId = Guid.NewGuid();
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid());
        var doctor = CreateDoctor(anotherDoctorId, command.InitiatorUserId);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedNoShow);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenDoctorProfileMissing()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    private Appointment CreateAppointment(Guid id, Guid doctorId)
    {
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1);
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );
        appointment.SetId(id);
        return appointment;
    }

    private static Doctor CreateDoctor(Guid id, Guid userId)
    {
        var doctor = Doctor.Create(
            userId,
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Room 1",
            ConsultationRoom.Create(1, "Room 1", 1)
        );
        doctor.SetId(id);
        return doctor;
    }
}
