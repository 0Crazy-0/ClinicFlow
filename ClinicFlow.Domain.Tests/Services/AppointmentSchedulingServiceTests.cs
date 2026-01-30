using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    private readonly Mock<IAppointmentRepository> _repositoryMock;
    private readonly AppointmentSchedulingService _service;

    public AppointmentSchedulingServiceTests()
    {
        _repositoryMock = new Mock<IAppointmentRepository>();
        _service = new AppointmentSchedulingService(_repositoryMock.Object);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenPatientIsBlocked()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = new Doctor(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateBlock(patient.Id, "Blocked", DateTime.UtcNow.AddDays(1))
        };

        // Act
        var act = () => _service.ScheduleAppointmentAsync(patient, penalties, doctor, DateTime.UtcNow.AddDays(1),
            new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10)), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<PatientBlockedException>();

        _repositoryMock.Verify(x => x.HasConflictAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = new Doctor(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        _repositoryMock.Setup(x => x.HasConflictAsync(doctor.Id, scheduledDate, timeRange.Start, timeRange.End))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.ScheduleAppointmentAsync(patient, penalties, doctor, scheduledDate, timeRange, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldReturnAppointment_WhenSuccess()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = new Doctor(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var appointmentTypeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.HasConflictAsync(doctor.Id, scheduledDate, timeRange.Start, timeRange.End)).ReturnsAsync(false);

        // Act
        var result = await _service.ScheduleAppointmentAsync(patient, penalties, doctor, scheduledDate, timeRange, appointmentTypeId);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patient.Id);
        result.DoctorId.Should().Be(doctor.Id);
        result.Status.Should().Be(AppointmentStatusEnum.Scheduled);
    }

    private Patient CreatePatient() => new(Guid.NewGuid(), DateTime.UtcNow.AddYears(-30), "O+", "None", "None", "Mom", "555-5555");
}
