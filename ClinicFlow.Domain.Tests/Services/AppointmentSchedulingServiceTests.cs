using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    // ScheduleAppointmentAsync
    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenPatientIsBlocked()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), Guid.NewGuid(), "Dr. House", 101);
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var penalties = new List<PatientPenalty> { PatientPenalty.CreateBlock(patient.Id, "Blocked", scheduledDate) };

        var context = new AppointmentSchedulingContext { Penalties = penalties, DoctorSchedule = null, HasConflict = false };
        var details = new AppointmentSchedulingDetails
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduledDate = scheduledDate,
            TimeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)),
            AppointmentTypeId = Guid.NewGuid()
        };

        var act = () => AppointmentSchedulingService.ScheduleAppointment(details, context);

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
        var context = new AppointmentSchedulingContext { Penalties = [], DoctorSchedule = null, HasConflict = false };
        var details = new AppointmentSchedulingDetails
        {
            PatientId = CreatePatient().Id,
            DoctorId = doctor.Id,
            ScheduledDate = scheduledDate,
            TimeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)),
            AppointmentTypeId = Guid.NewGuid()
        };

        var act = () => AppointmentSchedulingService.ScheduleAppointment(details, context);

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
        var context = new AppointmentSchedulingContext { Penalties = [], DoctorSchedule = schedule, HasConflict = false };
        var details = new AppointmentSchedulingDetails
        {
            PatientId = CreatePatient().Id,
            DoctorId = doctor.Id,
            ScheduledDate = scheduledDate,
            TimeRange = TimeRange.Create(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
            AppointmentTypeId = Guid.NewGuid()
        };

        var act = () => AppointmentSchedulingService.ScheduleAppointment(details, context); // Outside working hours

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
        var context = new AppointmentSchedulingContext { Penalties = penalties, DoctorSchedule = schedule, HasConflict = true };
        var details = new AppointmentSchedulingDetails
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduledDate = scheduledDate,
            TimeRange = timeRange,
            AppointmentTypeId = Guid.NewGuid()
        };

        var act = () => AppointmentSchedulingService.ScheduleAppointment(details, context);

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
        var context = new AppointmentSchedulingContext { Penalties = penalties, DoctorSchedule = schedule, HasConflict = false };
        var details = new AppointmentSchedulingDetails
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            ScheduledDate = scheduledDate,
            TimeRange = timeRange,
            AppointmentTypeId = appointmentTypeId
        };

        var result = AppointmentSchedulingService.ScheduleAppointment(details, context);

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
        var context = new AppointmentSchedulingContext { DoctorSchedule = null, ExistingAppointmentsDay = [] };

        var act = () => AppointmentSchedulingService.RescheduleAppointment(appointment, newDate, newTimeRange, context);

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
        var context = new AppointmentSchedulingContext { DoctorSchedule = schedule, ExistingAppointmentsDay = [conflictingAppointment] };

        var act = () => AppointmentSchedulingService.RescheduleAppointment(appointment, newDate, TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11)), context);

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
        var context = new AppointmentSchedulingContext { DoctorSchedule = schedule, ExistingAppointmentsDay = [] };

        AppointmentSchedulingService.RescheduleAppointment(appointment, newDate, newTimeRange, context);

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
        var context = new AppointmentSchedulingContext { DoctorSchedule = schedule, ExistingAppointmentsDay = [] };
        
        var act = () => AppointmentSchedulingService.RescheduleAppointment(appointment, newDate, newTimeRange, context);

        // Assert
        act.Should().Throw<AppointmentReschedulingNotAllowedException>().WithMessage(DomainErrors.Appointment.CannotReschedule);
    }

    // Helpers
    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static Patient CreatePatient() => Patient.CreateSelf(Guid.NewGuid(), PersonName.Create("Test Patient"), DateTime.UtcNow.AddYears(-30), BloodType.Create("O+"), "None", "None",
        EmergencyContact.Create("Mom", "555-5555"));

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange) => Schedule.Create(doctorId, dayOfWeek, timeRange);

    private static Schedule CreateValidSchedule(Guid doctorId, DayOfWeek dayOfWeek) => CreateSchedule(doctorId, dayOfWeek, TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(18)));

}

