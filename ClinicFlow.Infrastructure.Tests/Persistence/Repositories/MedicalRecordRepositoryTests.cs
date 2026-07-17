using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class MedicalRecordRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
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
        var record = MedicalRecord.Create(patient.Id, doctor.Id, appointment.Id, "chiefComplaint");

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
    public async Task GetByIdAsync_ShouldReturnMedicalRecord_WhenExists()
    {
        // Arrange
        var (doctor, patient, appointment) = await SeedCommonEntitiesAsync();
        var record = MedicalRecord.Create(patient.Id, doctor.Id, appointment.Id, "chiefComplaint");

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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
            "Patient 1 complaint"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient2.Id,
            doctor.Id,
            appointment2.Id,
            "Patient 2 complaint"
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
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnRecordsOrderedByIdDescending()
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor2.Id,
            appointment2.Id,
            "chiefComplaint 2"
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
    public async Task GetByDoctorIdPaginatedAsync_ShouldReturnRecordsOrderedByIdDescending()
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
            "chiefComplaint 1"
        );

        Context.MedicalRecords.Add(record1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record2 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment2.Id,
            "chiefComplaint 2"
        );

        Context.MedicalRecords.Add(record2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var record3 = MedicalRecord.Create(
            patient.Id,
            doctor.Id,
            appointment3.Id,
            "chiefComplaint 3"
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
        var record = MedicalRecord.Create(patient.Id, doctor.Id, appointment.Id, "chiefComplaint");

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

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-" + Guid.CreateVersion7().ToString("N")[..5]),
            specialty.Id,
            "Desc",
            ConsultationRoom.Create(10, "Room 10", 1)
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
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
            DateTime.UtcNow
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
            $"Consultation-{Guid.CreateVersion7():N}",
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
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
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
