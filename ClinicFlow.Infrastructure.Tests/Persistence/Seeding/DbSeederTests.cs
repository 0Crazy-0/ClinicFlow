using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Seeding;
using ClinicFlow.Infrastructure.Persistence.Seeding.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Seeding;

public class DbSeederTests(DbSeederFixture fixture) : IClassFixture<DbSeederFixture>, IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new(DateTimeOffset.UtcNow);

    public async Task InitializeAsync() => await fixture.Respawner.ResetAsync(fixture.DbConnection);

    public Task DisposeAsync() => Task.CompletedTask;

    private ApplicationDbContext Context => fixture.Context;
    private FakeTimeProvider FakeTime => _fakeTime;

    [Fact]
    public async Task SeedAsync_ShouldPopulateDatabaseWithCorrectCountsAndDistributions()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, FakeTime, CancellationToken.None);

        // Assert
        // 1. Specialties
        Context.MedicalSpecialties.IgnoreQueryFilters().Should().HaveCount(15);

        // 2. Clinical Form Templates
        Context.ClinicalFormTemplates.Should().HaveCount(19);

        // 3. Appointment Type Definitions
        Context.AppointmentTypes.IgnoreQueryFilters().Should().HaveCount(32);
        (await Context.AppointmentTypes.IgnoreQueryFilters().CountAsync(a => a.IsDeleted))
            .Should()
            .Be(2);

        // 4. Users
        var users = await Context.Users.ToListAsync();

        users.Should().HaveCount(166);
        users.Count(u => u.Role is UserRole.Admin).Should().Be(3);
        users.Count(u => u.Role is UserRole.Receptionist).Should().Be(8);
        users.Count(u => u.Role is UserRole.Doctor).Should().Be(35);
        users.Count(u => u.Role is UserRole.Patient).Should().Be(120);

        // Active vs Inactive Patients
        users.Count(u => u.Role is UserRole.Patient && u.IsActive).Should().Be(115);
        users.Count(u => u.Role is UserRole.Patient && !u.IsActive).Should().Be(5);

        // Variety checks for seeded users
        users.Count(u => u.LastLoginAt.HasValue).Should().BeGreaterThan(0);
        users.Count(u => !u.LastLoginAt.HasValue).Should().BeGreaterThan(0);

        users.Count(u => u.IsPhoneVerified).Should().BeGreaterThan(0);
        users.Count(u => !u.IsPhoneVerified).Should().BeGreaterThan(0);
        users
            .Count(u => u.IsPhoneVerified)
            .Should()
            .BeGreaterThan(users.Count(u => !u.IsPhoneVerified));

        users.Count(u => u.FailedLoginAttempts > 0).Should().BeGreaterThan(0);
        users.Count(u => u.FailedLoginAttempts is 0).Should().BeGreaterThan(0);

        users.Count(u => u.LockoutEnd.HasValue).Should().BeGreaterThan(0);
        users.Count(u => !u.LockoutEnd.HasValue).Should().BeGreaterThan(0);

        // 5. Doctors
        var doctors = await Context.Doctors.IgnoreQueryFilters().ToListAsync();

        doctors.Should().HaveCount(35);
        doctors.Count(d => d.IsDeleted).Should().Be(3);

        // 6. Patients
        var patients = await Context.Patients.IgnoreQueryFilters().ToListAsync();

        patients.Should().HaveCount(200);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Self).Should().Be(120);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Child).Should().Be(25);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Spouse).Should().Be(20);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Parent).Should().Be(15);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Sibling).Should().Be(10);
        patients.Count(p => p.RelationshipToUser is PatientRelationship.Other).Should().Be(10);

        // 7. Schedules
        var schedules = await Context.Schedules.ToListAsync();

        schedules.Should().HaveCount(175);

        var morningStart = TimeSpan.FromHours(7);
        var afternoonStart = TimeSpan.FromHours(13);
        var fullStart = TimeSpan.FromHours(7);
        var fullEnd = TimeSpan.FromHours(16);

        schedules
            .Count(s =>
                s.TimeRange.Start == morningStart && s.TimeRange.End == TimeSpan.FromHours(13)
            )
            .Should()
            .Be(21);
        schedules
            .Count(s =>
                s.TimeRange.Start == afternoonStart && s.TimeRange.End == TimeSpan.FromHours(19)
            )
            .Should()
            .Be(19);
        schedules
            .Count(s => s.TimeRange.Start == fullStart && s.TimeRange.End == fullEnd)
            .Should()
            .Be(27);

        // 8. Appointments
        var appointments = await Context.Appointments.ToListAsync();

        appointments.Should().HaveCount(500);
        appointments.Count(a => a.Status is AppointmentStatus.Completed).Should().Be(250);
        appointments.Count(a => a.Status is AppointmentStatus.Scheduled).Should().Be(100);
        appointments.Count(a => a.Status is AppointmentStatus.Cancelled).Should().Be(50);
        appointments.Count(a => a.Status is AppointmentStatus.LateCancellation).Should().Be(40);
        appointments.Count(a => a.Status is AppointmentStatus.NoShow).Should().Be(35);
        appointments.Count(a => a.Status is AppointmentStatus.CheckedIn).Should().Be(15);
        appointments.Count(a => a.Status is AppointmentStatus.InProgress).Should().Be(10);
        appointments.Count(a => a.RescheduleCount is 1).Should().BeGreaterThan(0);
        appointments.Count(a => a.RescheduleCount is 0).Should().BeGreaterThan(0);
        appointments.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.PatientNotes));
        appointments
            .Where(a =>
                a.Status
                    is AppointmentStatus.CheckedIn
                        or AppointmentStatus.InProgress
                        or AppointmentStatus.Completed
            )
            .Should()
            .OnlyContain(a => !string.IsNullOrWhiteSpace(a.ReceptionistNotes));

        appointments
            .Where(a =>
                a.Status
                    is not (
                        AppointmentStatus.CheckedIn
                        or AppointmentStatus.InProgress
                        or AppointmentStatus.Completed
                    )
            )
            .Should()
            .OnlyContain(a => string.IsNullOrWhiteSpace(a.ReceptionistNotes));

        // 9. Medical Records
        Context.MedicalRecords.Should().HaveCount(250);

        // 10. Penalties
        var penalties = await Context.PatientPenalties.ToListAsync();

        penalties.Count(p => p.Type is PenaltyType.Warning).Should().Be(85);
        penalties.Count(p => p.Type is PenaltyType.TemporaryBlock).Should().Be(5);
        penalties.Count(p => p.IsRemoved).Should().Be(10);
    }

    [Fact]
    public async Task SeedAsync_ShouldDeactivateSpecialtiesWithoutActiveDoctors()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var specialties = await Context.MedicalSpecialties.IgnoreQueryFilters().ToListAsync();

        var inactiveSpecialties = specialties.Where(s => s.IsDeleted).ToList();

        inactiveSpecialties.Should().HaveCount(2);
        inactiveSpecialties.Select(s => s.Name).Should().BeEquivalentTo("Orthopedics", "Neurology");

        var activeSpecialties = specialties.Where(s => !s.IsDeleted).ToList();

        activeSpecialties.Should().HaveCount(13);
    }

    [Fact]
    public async Task SeedAsync_ShouldCorrectlySoftDeleteClosedOrRemovedPatients()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var allPatients = await Context.Patients.IgnoreQueryFilters().ToListAsync();

        allPatients.Count(p => p.IsDeleted).Should().Be(13); // 5 closed self accounts + 8 removed family members
        allPatients.Count(p => !p.IsDeleted).Should().Be(187);
        allPatients
            .Should()
            .AllSatisfy(p =>
            {
                p.BloodType.Should().NotBeNull();
                p.EmergencyContact.Should().NotBeNull();
                p.EmergencyContact.Name.Should().NotBeNull();
                p.EmergencyContact.PhoneNumber.Should().NotBeNull();
                p.DateOfBirth.Should().BeBefore(_fakeTime.GetUtcNow().UtcDateTime);
            });
    }

    [Fact]
    public async Task SeedAsync_ShouldDeactivateSpecifiedAppointmentTypes()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var appointmentTypes = await Context.AppointmentTypes.IgnoreQueryFilters().ToListAsync();
        var deactivatedTypes = appointmentTypes.Where(a => a.IsDeleted).ToList();

        deactivatedTypes.Should().HaveCount(2);
        deactivatedTypes
            .Select(t => t.Name)
            .Should()
            .BeEquivalentTo("Chronic Digestive Disease Review", "Mental Health Crisis Evaluation");

        var activeTypes = appointmentTypes.Where(a => !a.IsDeleted).ToList();

        activeTypes.Should().HaveCount(30);
    }

    [Fact]
    public async Task SeedAsync_ShouldGenerateValidSchedulesForDoctors()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var schedules = await Context.Schedules.ToListAsync();

        schedules.Should().HaveCount(175);
        schedules
            .Should()
            .AllSatisfy(s =>
            {
                s.DoctorId.Should().NotBeEmpty();
                s.TimeRange.Should().NotBeNull();
                s.TimeRange.Start.Should().BeLessThan(s.TimeRange.End);
            });

        var inactiveCount = schedules.Count(s => !s.IsActive);

        inactiveCount.Should().Be(20);
    }

    [Fact]
    public async Task SeedAsync_ShouldCorrectlyTransitionAppointmentsToTargetStatuses()
    {
        //Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var appointments = await Context.Appointments.ToListAsync();
        var completed = appointments.Where(a => a.Status is AppointmentStatus.Completed).ToList();
        completed.Should().HaveCount(250);
        completed
            .Should()
            .AllSatisfy(a =>
            {
                a.CheckedInAt.Should().NotBeNull();
            });

        var scheduled = appointments.Where(a => a.Status is AppointmentStatus.Scheduled).ToList();
        scheduled.Should().HaveCount(100);
        scheduled
            .Should()
            .AllSatisfy(a =>
            {
                a.CheckedInAt.Should().BeNull();
                a.CancelledAt.Should().BeNull();
            });

        var cancelled = appointments.Where(a => a.Status is AppointmentStatus.Cancelled).ToList();
        cancelled.Should().HaveCount(50);

        var systemCancelled = cancelled.Where(a => a.CancelledByUserId == null).ToList();
        systemCancelled.Should().HaveCount(10);
        systemCancelled
            .Should()
            .AllSatisfy(a =>
            {
                a.CancelledAt.Should().NotBeNull();
                a.CancellationReason.Should().Be(Appointment.SystemTimeoutCancellationReason);
            });

        var userCancelled = cancelled.Where(a => a.CancelledByUserId != null).ToList();
        userCancelled.Should().HaveCount(40);
        userCancelled
            .Should()
            .AllSatisfy(a =>
            {
                a.CancelledAt.Should().NotBeNull();
                a.CancellationReason.Should().NotBeNullOrWhiteSpace();
                a.CancellationReason.Should().NotBe(Appointment.SystemTimeoutCancellationReason);
            });

        var lateCancelled = appointments
            .Where(a => a.Status is AppointmentStatus.LateCancellation)
            .ToList();

        lateCancelled.Should().HaveCount(40);
        lateCancelled
            .Should()
            .AllSatisfy(a =>
            {
                a.CancelledAt.Should().NotBeNull();
                a.CancellationReason.Should().NotBeNullOrWhiteSpace();
                a.CancelledByUserId.Should().NotBeNull();
            });

        var noShow = appointments.Where(a => a.Status is AppointmentStatus.NoShow).ToList();
        noShow.Should().HaveCount(35);

        var checkedIn = appointments.Where(a => a.Status is AppointmentStatus.CheckedIn).ToList();
        checkedIn.Should().HaveCount(15);
        checkedIn
            .Should()
            .AllSatisfy(a =>
            {
                a.CheckedInAt.Should().NotBeNull();
            });

        var inProgress = appointments.Where(a => a.Status is AppointmentStatus.InProgress).ToList();
        inProgress.Should().HaveCount(10);
        inProgress
            .Should()
            .AllSatisfy(a =>
            {
                a.CheckedInAt.Should().NotBeNull();
            });
    }

    [Fact]
    public async Task SeedAsync_ShouldStructurePatientPenaltiesDeterministically()
    {
        // Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var penalties = await Context.PatientPenalties.ToListAsync();
        var appointments = await Context.Appointments.ToListAsync();

        penalties.Should().HaveCount(90);

        // All warnings must have an AppointmentId
        var warnings = penalties.Where(p => p.Type is PenaltyType.Warning).ToList();
        warnings.Should().HaveCount(85);
        warnings.Should().AllSatisfy(w => w.AppointmentId.Should().NotBeNull());

        // Every infraction appointment has at least one linked warning
        var infractionApptIds = appointments
            .Where(a => a.Status is AppointmentStatus.LateCancellation or AppointmentStatus.NoShow)
            .Select(a => a.Id)
            .ToHashSet();

        var warningApptIds = warnings.Select(w => w.AppointmentId!.Value).ToHashSet();
        infractionApptIds.Should().BeSubsetOf(warningApptIds);

        warnings
            .Should()
            .AllSatisfy(w =>
                w.Reason.Should().BeOneOf(PenaltyReasons.LateCancellation, PenaltyReasons.NoShow)
            );

        // Blocks should use AutomaticBlock reason and have BlockedUntil set
        var blocks = penalties.Where(p => p.Type is PenaltyType.TemporaryBlock).ToList();
        blocks.Should().HaveCount(5);
        blocks
            .Should()
            .AllSatisfy(b =>
            {
                b.Reason.Should().Be(PenaltyReasons.AutomaticBlock);
                b.BlockedUntil.Should().NotBeNull();
                b.AppointmentId.Should().BeNull();
                b.IsRemoved.Should().BeFalse();
            });

        // Removed warnings (10 staff-resolved)
        var removedWarnings = penalties.Where(p => p.IsRemoved).ToList();
        removedWarnings.Should().HaveCount(10);
        removedWarnings
            .Should()
            .AllSatisfy(w =>
            {
                w.Type.Should().Be(PenaltyType.Warning);
                w.AppointmentId.Should().NotBeNull();
            });
    }

    [Fact]
    public async Task SeedAsync_ShouldGenerateAppointmentsWithinDoctorSchedules()
    {
        // Arrange & Act
        await DbSeeder.SeedAsync(Context, _fakeTime, CancellationToken.None);

        // Assert
        var appointments = await Context.Appointments.ToListAsync();
        var schedules = await Context.Schedules.ToListAsync();

        appointments
            .Should()
            .AllSatisfy(appt =>
            {
                var doctorSchedule = schedules.FirstOrDefault(s =>
                    s.DoctorId == appt.DoctorId && s.DayOfWeek == appt.ScheduledDate.DayOfWeek
                );

                doctorSchedule.Should().NotBeNull();

                appt.TimeRange.Start.Should()
                    .BeGreaterThanOrEqualTo(doctorSchedule.TimeRange.Start);
                appt.TimeRange.End.Should().BeLessThanOrEqualTo(doctorSchedule.TimeRange.End);
            });
    }

    [Fact]
    public void ClinicalDetailSampleData_ShouldHaveEntriesForAllTemplateCodes()
    {
        // Arrange
        var templateCodes = ClinicalFormTemplateData.GetTemplates().Select(t => t.Code).ToList();
        var fallbackPayload =
            """{"additionalNotes":"Clinical details recorded during consultation."}""";

        // Act & Assert
        templateCodes
            .Should()
            .AllSatisfy(code =>
            {
                ClinicalDetailSampleData
                    .GetSamplePayload(code, 0)
                    .Should()
                    .NotBe(
                        fallbackPayload,
                        $"template code '{code}' should have sample data in ClinicalDetailSampleData"
                    );
            });
    }
}
