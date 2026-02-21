using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    private readonly AppointmentSchedulingService _sut;

    public AppointmentSchedulingServiceTests()
    {
        _sut = new AppointmentSchedulingService();
    }

    // ScheduleAppointmentAsync
    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenPatientIsBlocked()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var penalties = new List<PatientPenalty> { PatientPenalty.CreateBlock(patient.Id, "Blocked", scheduledDate) };

        // Act
        var act = () => _sut.ScheduleAppointment(patient, penalties, doctor, scheduledDate, TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)), Guid.NewGuid(), null, false);

        // Assert
        act.Should().Throw<PatientBlockedException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenDoctorHasNoSchedule()
    {
        // Arrange
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => _sut.ScheduleAppointment(CreatePatient(), [], doctor, scheduledDate, TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)),
            Guid.NewGuid(), null, false);

        // Assert
        act.Should().Throw<DoctorNotAvailableException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenOutsideDoctorSchedule()
    {
        // Arrange
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        var schedule = CreateSchedule(doctor.Id, scheduledDate.DayOfWeek, TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));

        // Act
        var act = () => _sut.ScheduleAppointment(CreatePatient(), [], doctor, scheduledDate,
            TimeRange.Create(TimeSpan.FromHours(17), TimeSpan.FromHours(18)), Guid.NewGuid(), schedule, false); // Outside working hours

        // Assert
        act.Should().Throw<DoctorNotAvailableException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        var schedule = CreateValidSchedule(doctor.Id, scheduledDate.DayOfWeek);

        // Act
        var act = () => _sut.ScheduleAppointment(patient, penalties, doctor, scheduledDate, timeRange, Guid.NewGuid(), schedule, true);

        // Assert
        act.Should().Throw<AppointmentConflictException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldReturnAppointment_WhenSuccess()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var appointmentTypeId = Guid.NewGuid();

        var schedule = CreateValidSchedule(doctor.Id, scheduledDate.DayOfWeek);

        // Act
        var result = _sut.ScheduleAppointment(patient, penalties, doctor, scheduledDate, timeRange, appointmentTypeId, schedule, false);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patient.Id);
        result.DoctorId.Should().Be(doctor.Id);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    // RescheduleAppointmentAsync
    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenDoctorNotAvailable()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Act
        var act = () => _sut.RescheduleAppointment(appointment, newDate, newTimeRange, null, []);

        // Assert
        act.Should().Throw<DoctorNotAvailableException>();
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);

        // Create a conflicting appointment
        var conflictingAppointment = CreateAppointment(newDate.Date.AddHours(10));

        var schedule = CreateValidSchedule(appointment.DoctorId, newDate.DayOfWeek);

        // Act
        var act = () => _sut.RescheduleAppointment(appointment, newDate, TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11)), schedule, [conflictingAppointment]);

        // Assert
        act.Should().Throw<AppointmentConflictException>();
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldSucceed_WhenNoConflict()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));

        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        var schedule = CreateValidSchedule(appointment.DoctorId, newDate.DayOfWeek);

        // Act
        _sut.RescheduleAppointment(appointment, newDate, newTimeRange, schedule, []);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.RescheduleCount.Should().Be(1);
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenReschedulingNotAllowed()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Simulate that the appointment has already been rescheduled once
        appointment.Reschedule(DateTime.UtcNow.AddDays(1).Date.AddHours(10), newTimeRange);

        var schedule = CreateValidSchedule(appointment.DoctorId, newDate.DayOfWeek);

        // Act
        var act = () => _sut.RescheduleAppointment(appointment, newDate, newTimeRange, schedule, []);

        // Assert
        act.Should().Throw<AppointmentReschedulingNotAllowedException>().WithMessage("Cannot reschedule appointment: This appointment cannot be rescheduled");
    }

    // Helpers
    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static Patient CreatePatient() => Patient.Create(Guid.NewGuid(), DateTime.UtcNow.AddYears(-30), BloodType.Create("O+"), "None", "None",
        EmergencyContact.Create("Mom", "555-5555"));

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange) => Schedule.Create(doctorId, dayOfWeek, timeRange);

    private Schedule CreateValidSchedule(Guid doctorId, DayOfWeek dayOfWeek) => CreateSchedule(doctorId, dayOfWeek, TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(18)));

}

