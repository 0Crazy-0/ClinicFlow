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

public class PatientPenaltyRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly PatientPenaltyRepository _sut = new(fixture.Context);
    private ApplicationDbContext Context => fixture.Context;

    public async ValueTask InitializeAsync()
    {
        await fixture.Respawner.ResetAsync(fixture.DbConnection);

        fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPenalty_WhenExists()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);
        var penalty = await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var result = await _sut.GetByIdAsync(penalty.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(penalty);
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
    public async Task CreateAsync_ShouldAddPenaltyToContext()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var penalty = PatientPenalty.CreateManualBlock(
            patient.Id,
            "Test block",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        await _sut.CreateAsync(penalty, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .PatientPenalties.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == penalty.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(penalty);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldAddMultiplePenaltiesToContext()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);
        var warning = PatientPenalty.CreateAutomaticWarning(
            patient.Id,
            appointment.Id,
            "Automatic warning"
        );

        var block = PatientPenalty.CreateManualBlock(
            patient.Id,
            "Manual block",
            BlockDuration.Minor,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        await _sut.CreateRangeAsync([warning, block], TestContext.Current.CancellationToken);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResults = await Context
            .PatientPenalties.AsNoTracking()
            .Where(p => p.PatientId == patient.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        dbResults.Should().BeEquivalentTo([warning, block]);
    }

    [Fact]
    public async Task GetHistoryByPatientIdAsync_ShouldReturnAllPenalties_IncludingRemovedAndExpired()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        var activeWarning = await CreateWarningAsync(patient.Id, appointment.Id);
        var removedWarning = await CreateWarningAsync(patient.Id, appointment.Id);

        removedWarning.Remove();

        var expiredBlock = await CreateExpiredBlockAsync(patient.Id);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetHistoryByPatientIdAsync(
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                [expiredBlock, removedWarning, activeWarning],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetHistoryByPatientIdAsync_ShouldReturnEmpty_WhenNoPenaltiesForPatient()
    {
        // Arrange
        var nonExistentPatientId = Guid.NewGuid();

        // Act
        var result = await _sut.GetHistoryByPatientIdAsync(
            nonExistentPatientId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryByPatientIdAsync_ShouldReturnOnlyPenaltiesFromRequestedPatient()
    {
        // Arrange
        var patient1 = await CreatePatientAsync();
        var patient2 = await CreatePatientAsync();
        var appointment1 = await CreateAppointmentAsync(patient1.Id);
        var appointment2 = await CreateAppointmentAsync(patient2.Id);

        var penalty1 = await CreateWarningAsync(patient1.Id, appointment1.Id);
        await CreateWarningAsync(patient2.Id, appointment2.Id);

        // Act
        var result = await _sut.GetHistoryByPatientIdAsync(
            patient1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(penalty1);
    }

    [Fact]
    public async Task GetHistoryByPatientIdPaginatedAsync_ShouldReturnPaginatedPenalties()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        await CreateWarningAsync(patient.Id, appointment.Id);
        var warning2 = await CreateWarningAsync(patient.Id, appointment.Id);
        var warning3 = await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var (items, totalCount) = await _sut.GetHistoryByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);
        items
            .Should()
            .BeEquivalentTo([warning3, warning2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetHistoryByPatientIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        var warning1 = await CreateWarningAsync(patient.Id, appointment.Id);
        await CreateWarningAsync(patient.Id, appointment.Id);
        await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var (items, totalCount) = await _sut.GetHistoryByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(warning1);
    }

    [Fact]
    public async Task GetHistoryByPatientIdPaginatedAsync_ShouldReturnEmpty_WhenNoPenaltiesForPatient()
    {
        // Arrange
        var nonExistentPatientId = Guid.NewGuid();

        // Act
        var (items, totalCount) = await _sut.GetHistoryByPatientIdPaginatedAsync(
            nonExistentPatientId,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryByPatientIdPaginatedAsync_ShouldIncludeRemovedAndExpiredPenalties()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        var removedWarning = await CreateWarningAsync(patient.Id, appointment.Id);

        removedWarning.Remove();

        var expiredBlock = await CreateExpiredBlockAsync(patient.Id);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetHistoryByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(2);

        items
            .Should()
            .BeEquivalentTo(
                [expiredBlock, removedWarning],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldReturnActiveBlocks_WhenBlockedUntilIsAfterReferenceDate()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var futureBlock = await CreateActiveBlockAsync(patient.Id);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(futureBlock);
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldExcludeRemovedBlocks()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var removedBlock = await CreateActiveBlockAsync(patient.Id, 10);

        removedBlock.Remove();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldExcludeExpiredBlocks()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        await CreateExpiredBlockAsync(patient.Id);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldExcludeWarnings()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldReturnEmpty_WhenNoPenaltiesForPatient()
    {
        // Arrange
        var nonExistentPatientId = Guid.NewGuid();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            nonExistentPatientId,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldReturnBlocksOrderedByBlockedUntilAscending()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var block1 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 15);
        var block2 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        var block3 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 10);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo([block2, block3, block1], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldExcludeBlocks_WhenBlockedUntilIsEqualToReferenceDate()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        await CreateActiveBlockAsync(patient.Id, daysFromNow: 0);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldReturnActiveBlocksAcrossAllPatients()
    {
        // Arrange
        var patient1 = await CreatePatientAsync();
        var patient2 = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var block1 = await CreateActiveBlockAsync(patient1.Id, daysFromNow: 10);

        await CreateActiveBlockAsync(patient2.Id, daysFromNow: 20);

        var block3 = await CreateActiveBlockAsync(patient1.Id, daysFromNow: 5);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([block3, block1], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        await CreateActiveBlockAsync(patient.Id, daysFromNow: 10);

        var block3 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 15);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(block3);
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldExcludeRemovedBlocks()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var removedBlock = await CreateActiveBlockAsync(patient.Id);

        removedBlock.Remove();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldExcludeExpiredBlocks()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        await CreateExpiredBlockAsync(patient.Id);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldExcludeBlocks_WhenBlockedUntilIsEqualToReferenceDate()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        await CreateActiveBlockAsync(patient.Id, daysFromNow: 0);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldReturnEmpty_WhenNoActiveBlocks()
    {
        // Arrange
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveWarningsPaginatedAsync_ShouldReturnActiveWarnings()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        await CreateWarningAsync(patient.Id, appointment.Id);
        var warning2 = await CreateWarningAsync(patient.Id, appointment.Id);
        var warning3 = await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var (items, totalCount) = await _sut.GetActiveWarningsPaginatedAsync(
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items
            .Should()
            .BeEquivalentTo([warning3, warning2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetActiveWarningsPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        var warning1 = await CreateWarningAsync(patient.Id, appointment.Id);
        await CreateWarningAsync(patient.Id, appointment.Id);
        await CreateWarningAsync(patient.Id, appointment.Id);

        // Act
        var (items, totalCount) = await _sut.GetActiveWarningsPaginatedAsync(
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(warning1);
    }

    [Fact]
    public async Task GetActiveWarningsPaginatedAsync_ShouldExcludeRemovedWarnings()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id);

        var removedWarning = await CreateWarningAsync(patient.Id, appointment.Id);
        removedWarning.Remove();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetActiveWarningsPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveWarningsPaginatedAsync_ShouldExcludeBlocks()
    {
        // Arrange
        var patient = await CreatePatientAsync();

        await CreateActiveBlockAsync(patient.Id);

        // Act
        var (items, totalCount) = await _sut.GetActiveWarningsPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveWarningsPaginatedAsync_ShouldReturnEmpty_WhenNoActiveWarnings()
    {
        //Arrange & Act
        var (items, totalCount) = await _sut.GetActiveWarningsPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBlocksByPatientIdAsync_ShouldReturnBlocksOrderedBySequenceNumberAscending_WhenBlockedUntilDatesAreEqual()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var block1 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        var block2 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        var block3 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);

        // Act
        var result = await _sut.GetActiveBlocksByPatientIdAsync(
            patient.Id,
            referenceDate,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo([block1, block2, block3], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetActiveBlocksPaginatedAsync_ShouldReturnBlocksOrderedBySequenceNumberAscending_WhenBlockedUntilDatesAreEqual()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var block1 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        var block2 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);
        var block3 = await CreateActiveBlockAsync(patient.Id, daysFromNow: 5);

        // Act
        var (items, totalCount) = await _sut.GetActiveBlocksPaginatedAsync(
            referenceDate,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);
        items
            .Should()
            .BeEquivalentTo([block1, block2, block3], options => options.WithStrictOrdering());
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

    private async Task<Doctor> CreateDoctorAsync()
    {
        var user = await CreateUserAsync(UserRole.Doctor);
        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);
        var roomNumber = Random.Shared.Next(
            ConsultationRoom.MinimumNumber,
            ConsultationRoom.MaximumNumber + 1
        );

        var floor = Random.Shared.Next(
            ConsultationRoom.MinimumFloor,
            ConsultationRoom.MaximumFloor + 1
        );

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-" + Guid.NewGuid().ToString("N")[..5]),
            specialty.Id,
            "Desc",
            ConsultationRoom.Create(roomNumber, $"Room {roomNumber}", floor)
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync();

        return doctor;
    }

    private async Task<Patient> CreatePatientAsync()
    {
        var user = await CreateUserAsync(UserRole.Patient);
        var patient = Patient.CreateSelf(
            user.Id,
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync();

        return patient;
    }

    private async Task<Appointment> CreateAppointmentAsync(Guid patientId)
    {
        var doctor = await CreateDoctorAsync();

        var apptType = AppointmentTypeDefinition.Create(
            AppointmentCategory.FirstConsultation,
            $"Consultation-{Guid.NewGuid():N}",
            "Desc",
            EncounterDuration.FromMinutes(20)
        );

        Context.AppointmentTypes.Add(apptType);

        await Context.SaveChangesAsync();

        var startMinute = Random.Shared.Next(0, 480);
        var appointment = Appointment.Schedule(
            patientId,
            doctor.Id,
            apptType.Id,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(
                new TimeOnly(8, 0).AddMinutes(startMinute),
                new TimeOnly(8, 0).AddMinutes(startMinute + 30)
            )
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync();

        return appointment;
    }

    private async Task<PatientPenalty> CreateWarningAsync(Guid patientId, Guid appointmentId)
    {
        var warning = PatientPenalty.CreateAutomaticWarning(
            patientId,
            appointmentId,
            "No-show warning"
        );

        Context.PatientPenalties.Add(warning);

        await Context.SaveChangesAsync();

        return warning;
    }

    private async Task<PatientPenalty> CreateActiveBlockAsync(Guid patientId, int daysFromNow = 1)
    {
        var referenceTime = _fakeTime
            .GetUtcNow()
            .UtcDateTime.AddDays(daysFromNow - (int)BlockDuration.Minor);

        var block = PatientPenalty.CreateManualBlock(
            patientId,
            "Manual block",
            BlockDuration.Minor,
            referenceTime
        );

        Context.PatientPenalties.Add(block);
        await Context.SaveChangesAsync();

        return block;
    }

    private async Task<PatientPenalty> CreateExpiredBlockAsync(Guid patientId)
    {
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime.AddDays(-100);

        var block = PatientPenalty.CreateManualBlock(
            patientId,
            "Manual block",
            BlockDuration.Minor,
            referenceTime
        );

        Context.PatientPenalties.Add(block);
        await Context.SaveChangesAsync();

        return block;
    }
}
