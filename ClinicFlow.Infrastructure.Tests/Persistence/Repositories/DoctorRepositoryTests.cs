using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class DoctorRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly DoctorRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddDoctorToContext()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        // Act
        await _sut.CreateAsync(doctor, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .Doctors.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == doctor.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDoctor_WhenExistsAndActive()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        Context.Doctors.Add(doctor);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(doctor.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        doctor.Suspend();

        Context.Doctors.Add(doctor);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(doctor.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
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
    public async Task GetByUserIdAsync_ShouldReturnDoctor_WhenExistsAndActive()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        doctor.Suspend();

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByUserIdAsync(
            nonExistentUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnPaginatedActiveDoctors_ForSpecialty()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        var user1 = await CreateUser();
        var user2 = await CreateUser();
        var user3 = await CreateUser();

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        Context.Doctors.Add(doctor1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor C"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        Context.Doctors.Add(doctor2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor3 = Doctor.Create(
            user3.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-33333"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(3, "Room 3", 1)
        );

        Context.Doctors.Add(doctor3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
            specialty.Id,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([doctor1, doctor3], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        var user1 = await CreateUser();
        var user2 = await CreateUser();
        var user3 = await CreateUser();

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        Context.Doctors.Add(doctor1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor C"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        Context.Doctors.Add(doctor2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor3 = Doctor.Create(
            user3.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-33333"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(3, "Room 3", 1)
        );

        Context.Doctors.Add(doctor3);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
            specialty.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(doctor2);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnOnlyDoctorsFromRequestedSpecialty()
    {
        // Arrange
        var specialty1 = await CreateSpecialty("Specialty 1");
        var specialty2 = await CreateSpecialty("Specialty 2");

        var user1 = await CreateUser();
        var user2 = await CreateUser();

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty1.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        Context.Doctors.Add(doctor1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty2.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        Context.Doctors.Add(doctor2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
            specialty1.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(doctor1);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldNotReturnSuspendedDoctors()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        var user1 = await CreateUser();
        var user2 = await CreateUser();

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        Context.Doctors.Add(doctor1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        doctor2.Suspend();

        Context.Doctors.Add(doctor2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
            specialty.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(doctor1);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldOrderBySequenceNumberAscending_WhenFullNamesAreEqual()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        var user1 = await CreateUser();
        var user2 = await CreateUser();

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor Same"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        Context.Doctors.Add(doctor1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor Same"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        Context.Doctors.Add(doctor2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
            specialty.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(2);

        items.Should().BeEquivalentTo([doctor1, doctor2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task HasActiveBySpecialtyIdAsync_ShouldReturnTrue_WhenActiveDoctorExistsForSpecialty()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveBySpecialtyIdAsync(
            specialty.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveBySpecialtyIdAsync_ShouldReturnFalse_WhenNoActiveDoctorForSpecialty()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        doctor.Suspend();

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveBySpecialtyIdAsync(
            specialty.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetIncludingDeletedByLicenseNumberAsync_ShouldReturnDoctor_WhenExistsAndActive()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetIncludingDeletedByLicenseNumberAsync(
            "CMP-12345",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetIncludingDeletedByLicenseNumberAsync_ShouldReturnDoctor_WhenSoftDeleted()
    {
        // Arrange
        var (user, specialty) = await CreatePrerequisitesAsync();
        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("CMP-12345"),
            specialty.Id,
            "Biography",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        doctor.Suspend();

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetIncludingDeletedByLicenseNumberAsync(
            "CMP-12345",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task GetIncludingDeletedByLicenseNumberAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentLicense = "CMP-NONEXIST";

        // Act
        var result = await _sut.GetIncludingDeletedByLicenseNumberAsync(
            nonExistentLicense,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    private async Task<User> CreateUser()
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Doctor
        );

        Context.Users.Add(user);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<MedicalSpecialty> CreateSpecialty(string name = "Specialty")
    {
        var specialty = MedicalSpecialty.Create(name, "Description", 30, 24);

        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return specialty;
    }

    private async Task<(User User, MedicalSpecialty Specialty)> CreatePrerequisitesAsync()
    {
        var user = await CreateUser();
        var specialty = await CreateSpecialty();

        return (user, specialty);
    }
}
