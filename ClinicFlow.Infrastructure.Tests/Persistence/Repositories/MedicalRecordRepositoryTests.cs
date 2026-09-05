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

public class MedicalRecordRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly MedicalRecordRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddMedicalRecordToContext()
    {
        // Arrange
        var (doctor, patient, appointment) = await SeedCommonEntitiesAsync();
        var record = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment.Id,
            "chiefComplaint",
            null
        );

        // Act
        await _sut.CreateAsync(record, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .MedicalRecords.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == record.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(record);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var (doctor, patient, appointment) = await SeedCommonEntitiesAsync();
        var record = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment.Id,
            "chiefComplaint",
            null
        );

        Context.MedicalRecords.Add(record);
        Context.Entry(record).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(record, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(record).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        // Arrange
        var (doctor, patient, appointment) = await SeedCommonEntitiesAsync();
        var record = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment.Id,
            "chiefComplaint",
            null
        );

        Context.MedicalRecords.Add(record);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(record.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(record);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnPaginatedRecords_ForPatient()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([record3, record2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(record1);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnOnlyRecordsFromRequestedPatient()
    {
        // Arrange
        var (doctor, patient1, _) = await SeedCommonEntitiesAsync();
        var patient2 = await CreatePatientAsync();

        var appointment1 = await CreateAppointmentAsync(patient1.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient2.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient1.Id,
            doctor.Id,
            appointment1.Id,
            "Patient 1 complaint",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient2.Id,
            doctor.Id,
            appointment2.Id,
            "Patient 2 complaint",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient1.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(record1);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnEmpty_WhenNoRecordsForPatient()
    {
        // Arrange
        var nonExistentPatientId = Guid.CreateVersion7();

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
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
    public async Task GetByPatientIdPaginatedExcludingCategoriesAsync_ShouldExcludeBlockedCategories_AndKeepUnprotectedRecords()
    {
        // Arrange
        // The seeded patient is an adult (30), so in production ProtectedCategoryPolicy
        // .GetProtectedCategoriesFor would return an empty exclusion list and no category
        // would ever be blocked. A blocked category is seeded here only to test the
        // repository's exclusion logic in isolation.
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var unprotectedRecord = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "unprotected complaint",
            null
        );

        var blockedRecord = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "blocked complaint",
            ProtectedCategory.MentalHealthCounseling
        );

        var allowedProtectedRecord = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "allowed protected complaint",
            ProtectedCategory.BuprenorphineOpioidTreatment
        );

        Context.MedicalRecords.Add(unprotectedRecord);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(blockedRecord);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(allowedProtectedRecord);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedExcludingCategoriesAsync(
            patient.Id,
            [ProtectedCategory.MentalHealthCounseling],
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(2);

        items
            .Should()
            .BeEquivalentTo(
                [allowedProtectedRecord, unprotectedRecord],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetByPatientIdPaginatedExcludingCategoriesAsync_ShouldPaginateExcludedResults()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedExcludingCategoriesAsync(
            patient.Id,
            [ProtectedCategory.MentalHealthCounseling],
            pageNumber: 2,
            pageSize: 1,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(2);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(record1);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedExcludingCategoriesAsync_ShouldSkipFullPages_WhenRequestingLaterPage()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment4 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        var record4 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment4.Id,
            "chiefComplaint 4",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Context.MedicalRecords.Add(record4);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedExcludingCategoriesAsync(
            patient.Id,
            [ProtectedCategory.MentalHealthCounseling],
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(4);

        items.Should().BeEquivalentTo([record2, record1], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByPatientIdPaginatedExcludingCategoriesAsync_ShouldReturnEmpty_WhenEveryRecordIsBlocked()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment.Id,
            "blocked complaint",
            ProtectedCategory.MentalHealthCounseling
        );

        Context.MedicalRecords.Add(record);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedExcludingCategoriesAsync(
            patient.Id,
            [ProtectedCategory.MentalHealthCounseling],
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnRecordsOrderedBySequenceNumberDescending()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();
        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, _) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        items
            .Should()
            .BeEquivalentTo([record3, record2, record1], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnPaginatedRecords_ForDoctor()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor.Id,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([record3, record2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(record1);
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnOnlyRecordsFromRequestedDoctor()
    {
        // Arrange
        var (doctor1, patient, _) = await SeedCommonEntitiesAsync();
        var doctor2 = await CreateDoctorAsync();

        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor1.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor2.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor1.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor2.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor1.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(record1);
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnEmpty_WhenNoRecordsForDoctor()
    {
        // Arrange
        var nonExistentDoctorId = Guid.CreateVersion7();

        // Act
        var (items, totalCount) = await _sut.GetByDoctorIdPaginatedAsync(
            nonExistentDoctorId,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnRecordsOrderedBySequenceNumberDescending()
    {
        // Arrange
        var (doctor, patient, _) = await SeedCommonEntitiesAsync();
        var appointment1 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment2 = await CreateAppointmentAsync(patient.Id, doctor.Id);
        var appointment3 = await CreateAppointmentAsync(patient.Id, doctor.Id);

        var record1 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment1.Id,
            "chiefComplaint 1",
            null
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2",
            null
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3",
            null
        );

        Context.MedicalRecords.Add(record3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, _) = await _sut.GetByDoctorIdPaginatedAsync(
            doctor.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        items
            .Should()
            .BeEquivalentTo([record3, record2, record1], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByAppointmentIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        // Arrange
        var (doctor, patient, appointment) = await SeedCommonEntitiesAsync();
        var record = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment.Id,
            "chiefComplaint",
            null
        );

        Context.MedicalRecords.Add(record);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByAppointmentIdAsync(
            appointment.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(record);
    }

    [Fact]
    public async Task GetByAppointmentIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentAppointmentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByAppointmentIdAsync(
            nonExistentAppointmentId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    private async Task<(
        Doctor Doctor,
        Patient Patient,
        Appointment Appointment
    )> SeedCommonEntitiesAsync()
    {
        var doctor = await CreateDoctorAsync();
        var patient = await CreatePatientAsync();
        var appointment = await CreateAppointmentAsync(patient.Id, doctor.Id);

        return (doctor, patient, appointment);
    }

    private async Task<User> CreateUserAsync(UserRole role)
    {
        var email = EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com");
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
            MedicalLicenseNumber.Create("LicenseNumber"),
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
        var patient = Patient.CreateProfile(
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

    private async Task<Appointment> CreateAppointmentAsync(Guid patientId, Guid doctorId)
    {
        var apptType = AppointmentTypeDefinition.Create(
            AppointmentCategory.FirstConsultation,
            "name",
            "Desc",
            EncounterDuration.FromMinutes(20)
        );

        Context.AppointmentTypes.Add(apptType);

        await Context.SaveChangesAsync();

        var startMinute = Random.Shared.Next(0, 480);
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
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
}
