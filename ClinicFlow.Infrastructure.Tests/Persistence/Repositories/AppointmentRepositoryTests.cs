using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class AppointmentRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AppointmentRepository _sut;
    private ApplicationDbContext Context => _fixture.Context;

    public AppointmentRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _sut = new AppointmentRepository(fixture.Context, _fakeTime);
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.Respawner.ResetAsync(_fixture.DbConnection);

        _fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAppointment_WhenExists()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, doctor, patient) = await CreateAppointmentDraftAsync(
            scheduledDate,
            timeRange
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(appointment.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointment.Id);
        result.PatientId.Should().Be(patient.Id);
        result.DoctorId.Should().Be(doctor.Id);
        result.AppointmentTypeId.Should().Be(appointment.AppointmentTypeId);
        result.ScheduledDate.Should().Be(scheduledDate);
        result.TimeRange.Start.Should().Be(timeRange.Start);
        result.TimeRange.End.Should().Be(timeRange.End);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnPaginatedAppointments_ForDoctorOnSpecificDate()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        var appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.AddRange(appointment1, appointment2, appointment3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor.Id,
            scheduledDate,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().HaveCount(2);
        items[0].Id.Should().Be(appointment1.Id);
        items[1].Id.Should().Be(appointment3.Id);
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnOnlyAppointmentsForRequestedDoctor()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (appointmentDoctor1, doctor1, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var (appointmentDoctor2, _, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        Context.Appointments.AddRange(appointmentDoctor1, appointmentDoctor2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor1.Id,
            scheduledDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().HaveCount(1);
        items[0].Id.Should().Be(appointmentDoctor1.Id);
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnSecondPage_ForDoctorOnSpecificDate()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        var appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.AddRange(appointment1, appointment2, appointment3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor.Id,
            scheduledDate,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().HaveCount(1);
        items[0].Id.Should().Be(appointment2.Id);
    }

    [Fact]
    public async Task GetByDateRangePaginatedAsync_ShouldReturnAppointments_WithinRange()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var Appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var Appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );
        var Appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(2),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var AppointmentOutOfRange = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(5),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.AddRange(
            Appointment1,
            Appointment2,
            Appointment3,
            AppointmentOutOfRange
        );

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDateRangePaginatedAsync(
            baseDate.AddDays(1),
            baseDate.AddDays(2),
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().HaveCount(3);
        items[0].Id.Should().Be(Appointment1.Id);
        items[1].Id.Should().Be(Appointment2.Id);
        items[2].Id.Should().Be(Appointment3.Id);
    }

    [Fact]
    public async Task GetByDateRangePaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var Appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var Appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );
        var Appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(2),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.AddRange(Appointment1, Appointment2, Appointment3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDateRangePaginatedAsync(
            baseDate.AddDays(1),
            baseDate.AddDays(2),
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);
        items.Should().HaveCount(1);
        items[0].Id.Should().Be(Appointment3.Id);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnAppointments_ForPatient()
    {
        // Arrange
        var (doctor, patient1, apptType) = await SeedCommonEntitiesAsync();
        var patientUser2 = await CreateUserAsync(UserRole.Patient);
        var patient2 = await CreatePatientAsync(patientUser2.Id);
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var Appointment1 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var Appointment2 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );
        var Appointment3 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(2),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var AppointmentOtherPatient = Appointment.Schedule(
            patient2.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.AddRange(
            Appointment1,
            Appointment2,
            Appointment3,
            AppointmentOtherPatient
        );

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient1.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().HaveCount(3);
        items[0].Id.Should().Be(Appointment1.Id);
        items[1].Id.Should().Be(Appointment2.Id);
        items[2].Id.Should().Be(Appointment3.Id);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnSecondPage_ForPatient()
    {
        // Arrange
        var (doctor, patient1, apptType) = await SeedCommonEntitiesAsync();
        var patientUser2 = await CreateUserAsync(UserRole.Patient);
        var patient2 = await CreatePatientAsync(patientUser2.Id);
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var Appointment1 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var Appointment2 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );
        var Appointment3 = Appointment.Schedule(
            patient1.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(2),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var AppointmentOtherPatient = Appointment.Schedule(
            patient2.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.AddRange(
            Appointment1,
            Appointment2,
            Appointment3,
            AppointmentOtherPatient
        );

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient1.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);
        items.Should().HaveCount(1);
        items[0].Id.Should().Be(Appointment3.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAppointmentToDbContext()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, doctor, patient) = await CreateAppointmentDraftAsync(
            scheduledDate,
            timeRange
        );

        // Act
        await _sut.CreateAsync(appointment, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbAppt = await Context
            .Appointments.AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == appointment.Id,
                TestContext.Current.CancellationToken
            );

        dbAppt.Should().NotBeNull();
        dbAppt.PatientId.Should().Be(patient.Id);
        dbAppt.DoctorId.Should().Be(doctor.Id);
        dbAppt.AppointmentTypeId.Should().Be(appointment.AppointmentTypeId);
        dbAppt.ScheduledDate.Should().Be(scheduledDate);
        dbAppt.TimeRange.Should().Be(timeRange);
        dbAppt.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnTrue_WhenActiveFutureAppointmentExists()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, _, patient) = await CreateAppointmentDraftAsync(scheduledDate, timeRange);

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnFalse_WhenNoFutureAppointments()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(-1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, _, patient) = await CreateAppointmentDraftAsync(scheduledDate, timeRange);

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnTrue_WhenAppointmentIsCheckedIn()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, _, patient) = await CreateAppointmentDraftAsync(scheduledDate, timeRange);
        appointment.CheckIn(scheduledDate);

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnTrue_WhenAppointmentRequiresReassignment()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var (appointment, _, patient) = await CreateAppointmentDraftAsync(scheduledDate, timeRange);
        appointment.MarkAsRequiresReassignment();

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnFalse_WhenAppointmentIsCancelled()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var (appointment, _, patient) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        appointment.Cancel(patient.UserId, null, scheduledDate);

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnFalse_WhenAppointmentStartsExactlyNow()
    {
        // Arrange
        var now = _fakeTime.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var (appointment, _, patient) = await CreateAppointmentDraftAsync(
            today,
            TimeRange.Create(nowTime, nowTime.AddHours(1))
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveAppointmentsForUserAsync_ShouldReturnFalse_WhenAppointmentStartsInPastToday()
    {
        // Arrange
        var tomorrowNoon = new DateTimeOffset(
            _fakeTime.GetUtcNow().UtcDateTime.Date.AddDays(1).AddHours(12),
            TimeSpan.Zero
        );

        _fakeTime.SetUtcNow(tomorrowNoon);

        var (appointment, _, patient) = await CreateAppointmentDraftAsync(
            DateOnly.FromDateTime(tomorrowNoon.UtcDateTime),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(9, 30))
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveAppointmentsForUserAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnTrue_WhenOverlappingAppointmentExists()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeTrue();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenNoOverlappingAppointment()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenNewAppointmentEndsExactlyWhenExistingBegins()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenOverlappingAppointmentIsOnDifferentDate()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var differentDate = scheduledDate.AddDays(1);

        var (existingAppointment, doctor, _) = await CreateAppointmentDraftAsync(
            differentDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenOverlappingAppointmentIsForDifferentDoctor()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, _, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctorUserA = await CreateUserAsync(UserRole.Doctor);
        var doctorA = await CreateDoctorAsync(doctorUserA.Id);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctorA.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenOverlappingAppointmentIsCancelled()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, patient) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );
        existingAppointment.Cancel(patient.UserId, "Reason", scheduledDate);

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenOverlappingAppointmentIsLateCancelled()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, patient) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        existingAppointment.CancelLate(patient.UserId, "Late Cancelled", scheduledDate);

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_ShouldReturnFalse_WhenOverlappingAppointmentRequiresReassignment()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var (existingAppointment, doctor, _) = await CreateAppointmentDraftAsync(
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );
        existingAppointment.MarkAsRequiresReassignment();

        Context.Appointments.Add(existingAppointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var hasConflict = await _sut.HasConflictAsync(
            doctor.Id,
            scheduledDate,
            TimeRange.Create(new TimeOnly(10, 30), new TimeOnly(11, 30)),
            TestContext.Current.CancellationToken
        );

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public async Task GetFutureScheduledByDoctorIdAsync_ShouldReturnOnlyFutureScheduledAppointments()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var Appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var Appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(-1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        var Appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        Appointment3.Cancel(Guid.NewGuid(), "cancelled", baseDate);

        Context.Appointments.AddRange(Appointment1, Appointment2, Appointment3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetFutureScheduledByDoctorIdAsync(
            doctor.Id,
            baseDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle();
        results[0].Id.Should().Be(Appointment1.Id);
    }

    [Fact]
    public async Task GetFutureScheduledByDoctorIdAsync_ShouldReturnAppointment_WhenScheduledDateEqualsReferenceDate()
    {
        // Arrange
        var baseDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var (appointment, doctor, _) = await CreateAppointmentDraftAsync(
            baseDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetFutureScheduledByDoctorIdAsync(
            doctor.Id,
            baseDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle();
        results[0].Id.Should().Be(appointment.Id);
    }

    [Fact]
    public async Task GetExpiredDisplacedAppointmentsAsync_ShouldReturnRequiresReassignmentAppointmentsInPast()
    {
        // Arrange
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();
        var baseTime = _fakeTime.GetUtcNow().UtcDateTime;
        var baseDate = DateOnly.FromDateTime(baseTime);

        var Appointment1 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Appointment1.MarkAsRequiresReassignment();

        var Appointment2 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate.AddDays(1),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Appointment2.MarkAsRequiresReassignment();

        var Appointment3 = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            baseDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Appointments.AddRange(Appointment1, Appointment2, Appointment3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetExpiredDisplacedAppointmentsAsync(
            baseTime.Date.AddHours(12),
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle();
        results[0].Id.Should().Be(Appointment1.Id);
    }

    [Fact]
    public async Task GetExpiredDisplacedAppointmentsAsync_ShouldNotReturnAppointment_WhenScheduledForTodayInFuture()
    {
        // Arrange
        var baseTime = _fakeTime.GetUtcNow().UtcDateTime;
        var baseDate = DateOnly.FromDateTime(baseTime);
        var (appointment, _, _) = await CreateAppointmentDraftAsync(
            baseDate,
            TimeRange.Create(new TimeOnly(13, 0), new TimeOnly(14, 0))
        );

        appointment.MarkAsRequiresReassignment();

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetExpiredDisplacedAppointmentsAsync(
            baseTime.Date.AddHours(12),
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExpiredDisplacedAppointmentsAsync_ShouldNotReturnAppointment_WhenStartTimeEqualsReferenceTime()
    {
        // Arrange
        var baseTime = _fakeTime.GetUtcNow().UtcDateTime;
        var baseDate = DateOnly.FromDateTime(baseTime);
        var (appointment, _, _) = await CreateAppointmentDraftAsync(
            baseDate,
            TimeRange.Create(new TimeOnly(13, 0), new TimeOnly(14, 0))
        );

        appointment.MarkAsRequiresReassignment();

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetExpiredDisplacedAppointmentsAsync(
            baseTime.Date.AddHours(13),
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    private async Task<(
        Appointment Appointment,
        Doctor Doctor,
        Patient Patient
    )> CreateAppointmentDraftAsync(DateOnly scheduledDate, TimeRange timeRange)
    {
        var (doctor, patient, apptType) = await SeedCommonEntitiesAsync();

        var appointment = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            scheduledDate,
            timeRange
        );

        return (appointment, doctor, patient);
    }

    private async Task<(
        Doctor Doctor,
        Patient Patient,
        AppointmentTypeDefinition AppointmentType
    )> SeedCommonEntitiesAsync()
    {
        var doctorUser = await CreateUserAsync(UserRole.Doctor);
        var doctor = await CreateDoctorAsync(doctorUser.Id);
        var patientUser = await CreateUserAsync(UserRole.Patient);
        var patient = await CreatePatientAsync(patientUser.Id);
        var apptType = AppointmentTypeDefinition.Create(
            AppointmentCategory.FirstConsultation,
            "Standard Consultation",
            "Desc",
            EncounterDuration.FromMinutes(20)
        );

        Context.AppointmentTypes.Add(apptType);

        await Context.SaveChangesAsync();

        return (doctor, patient, apptType);
    }

    private async Task<User> CreateUserAsync(UserRole role)
    {
        var email = EmailAddress.Create($"{Guid.NewGuid()}@clinic.com");
        var phone = PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}");
        var user = User.Create(email, "password", phone, role);

        Context.Users.Add(user);

        await Context.SaveChangesAsync();

        return user;
    }

    private async Task<Doctor> CreateDoctorAsync(Guid userId)
    {
        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        var doctor = Doctor.Create(
            userId,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-" + Guid.NewGuid().ToString("N")[..5]),
            specialty.Id,
            "Desc",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync();

        return doctor;
    }

    private async Task<Patient> CreatePatientAsync(Guid userId)
    {
        var refTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(refTime.AddYears(-30)),
            refTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync();

        return patient;
    }
}
