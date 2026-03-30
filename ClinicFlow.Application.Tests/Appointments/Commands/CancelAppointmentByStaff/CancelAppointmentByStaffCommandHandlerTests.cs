using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointmentByStaff;

public class CancelAppointmentByStaffCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IMedicalSpecialtyRepository> _specialtyRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CancelAppointmentByStaffCommandHandler _sut;

    public CancelAppointmentByStaffCommandHandlerTests()
    {
        _sut = new CancelAppointmentByStaffCommandHandler(
            _fakeTime,
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _specialtyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff reason"
        );

        var doctorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var doctor = CreateDoctor(doctorId, specialtyId);
        var specialty = CreateSpecialty(specialtyId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _specialtyRepositoryMock
            .Setup(r => r.GetByIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(
            r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff reason"
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff reason"
        );

        var doctorId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Staff reason"
        );

        var doctorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var doctor = CreateDoctor(doctorId, specialtyId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _specialtyRepositoryMock
            .Setup(r => r.GetByIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalSpecialty?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid typeId,
        DateTime referenceTime
    )
    {
        var scheduledDate = referenceTime.AddDays(2).Date;
        var timeRange = TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            scheduledDate,
            timeRange
        );
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        return appointment;
    }

    private static Doctor CreateDoctor(Guid id, Guid specialtyId)
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("1234567"),
            specialtyId,
            "555-1234",
            101
        );
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
    }

    private static MedicalSpecialty CreateSpecialty(Guid id, int minCancellationHours = 24)
    {
        var specialty = (MedicalSpecialty)Activator.CreateInstance(typeof(MedicalSpecialty), true)!;
        SetPrivateProperty(specialty, nameof(MedicalSpecialty.Id), id);
        SetPrivateProperty(
            specialty,
            nameof(MedicalSpecialty.MinCancellationHours),
            minCancellationHours
        );
        return specialty;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }
}
