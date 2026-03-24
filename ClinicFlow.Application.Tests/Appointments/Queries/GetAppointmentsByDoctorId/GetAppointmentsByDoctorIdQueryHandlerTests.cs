using System.Reflection;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByDoctorIdQueryHandler _sut;

    public GetAppointmentsByDoctorIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByDoctorIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenDoctorHasNoAppointmentsOnDate()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid(), DateTime.UtcNow.Date);

        _appointmentRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(query.DoctorId, query.Date))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _appointmentRepositoryMock.Verify(
            x => x.GetByDoctorIdAsync(query.DoctorId, query.Date),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentList_WhenDoctorHasAppointmentsOnDate()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var query = new GetAppointmentsByDoctorIdQuery(doctorId, date);

        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), doctorId, date),
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), doctorId, date),
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, date))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AppointmentDto>();
        result.Select(x => x.DoctorId).Should().AllBeEquivalentTo(doctorId);

        _appointmentRepositoryMock.Verify(x => x.GetByDoctorIdAsync(doctorId, date), Times.Once);
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        DateTime scheduledDate
    )
    {
        var timeRange = TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        SetPrivateProperty(appointment, nameof(Appointment.PatientId), patientId);
        SetPrivateProperty(appointment, nameof(Appointment.DoctorId), doctorId);
        SetPrivateProperty(appointment, nameof(Appointment.AppointmentTypeId), Guid.NewGuid());
        SetPrivateProperty(appointment, nameof(Appointment.ScheduledDate), scheduledDate);
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
