using System.Reflection;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByPatientIdQueryHandler _sut;

    public GetAppointmentsByPatientIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByPatientIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenPatientHasNoAppointments()
    {
        // Arrange
        var query = new GetAppointmentsByPatientIdQuery(Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(query.PatientId))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _appointmentRepositoryMock.Verify(x => x.GetByPatientIdAsync(query.PatientId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentList_WhenPatientHasAppointments()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientIdQuery(patientId);

        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), patientId, Guid.NewGuid()),
            CreateAppointment(Guid.NewGuid(), patientId, Guid.NewGuid()),
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AppointmentDto>();
        result.Select(x => x.PatientId).Should().AllBeEquivalentTo(patientId);

        _appointmentRepositoryMock.Verify(x => x.GetByPatientIdAsync(patientId), Times.Once);
    }

    private static Appointment CreateAppointment(Guid id, Guid patientId, Guid doctorId)
    {
        var timeRange = TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        SetPrivateProperty(appointment, nameof(Appointment.PatientId), patientId);
        SetPrivateProperty(appointment, nameof(Appointment.DoctorId), doctorId);
        SetPrivateProperty(appointment, nameof(Appointment.AppointmentTypeId), Guid.NewGuid());
        SetPrivateProperty(
            appointment,
            nameof(Appointment.ScheduledDate),
            DateTime.UtcNow.Date.AddDays(1)
        );
        SetPrivateProperty(appointment, nameof(Appointment.TimeRange), timeRange);
        return appointment;
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
