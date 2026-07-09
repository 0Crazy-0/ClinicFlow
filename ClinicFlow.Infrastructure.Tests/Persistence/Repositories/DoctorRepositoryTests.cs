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
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(user.Id);
        result.FullName.Should().Be(doctor.FullName);
        result.MedicalSpecialtyId.Should().Be(specialty.Id);
        result.LicenseNumber.Value.Should().Be(doctor.LicenseNumber.Value);
        result.Biography.Should().Be(doctor.Biography);
        result.ConsultationRoom.Should().Be(doctor.ConsultationRoom);
        result.IsDeleted.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(user.Id);
        result.FullName.Should().Be(doctor.FullName);
        result.MedicalSpecialtyId.Should().Be(specialty.Id);
        result.LicenseNumber.Value.Should().Be(doctor.LicenseNumber.Value);
        result.Biography.Should().Be(doctor.Biography);
        result.ConsultationRoom.Should().Be(doctor.ConsultationRoom);
        result.IsDeleted.Should().BeFalse();
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
        var specialty = CreateSpecialty();

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1 = CreateUser();
        var user2 = CreateUser();
        var user3 = CreateUser();

        Context.Users.AddRange(user1, user2, user3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor C"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        var doctor3 = Doctor.Create(
            user3.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-33333"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(3, "Room 3", 1)
        );

        Context.Doctors.AddRange(doctor1, doctor2, doctor3);

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

        items.Should().HaveCount(2);
        items[0].Id.Should().Be(doctor1.Id);
        items[1].Id.Should().Be(doctor3.Id);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var specialty = CreateSpecialty();

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1 = CreateUser();
        var user2 = CreateUser();
        var user3 = CreateUser();

        Context.Users.AddRange(user1, user2, user3);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor C"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        var doctor3 = Doctor.Create(
            user3.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-33333"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(3, "Room 3", 1)
        );

        Context.Doctors.AddRange(doctor1, doctor2, doctor3);

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

        items.Should().HaveCount(1);
        items[0].Id.Should().Be(doctor2.Id);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnOnlyDoctorsFromRequestedSpecialty()
    {
        // Arrange
        var specialty1 = CreateSpecialty("Specialty 1");
        var specialty2 = CreateSpecialty("Specialty 2");

        Context.MedicalSpecialties.AddRange(specialty1, specialty2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1 = CreateUser();
        var user2 = CreateUser();

        Context.Users.AddRange(user1, user2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty1.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );
        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty2.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );
        Context.Doctors.AddRange(doctor1, doctor2);
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

        items.Should().ContainSingle().Which.Id.Should().Be(doctor1.Id);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldNotReturnSuspendedDoctors()
    {
        // Arrange
        var specialty = CreateSpecialty();

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1 = CreateUser();
        var user2 = CreateUser();

        Context.Users.AddRange(user1, user2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor A"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor B"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        doctor2.Suspend();

        Context.Doctors.AddRange(doctor1, doctor2);

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

        items.Should().ContainSingle().Which.Id.Should().Be(doctor1.Id);
    }

    [Fact]
    public async Task GetBySpecialtyIdPaginatedAsync_ShouldOrderByIdAscending_WhenFullNamesAreEqual()
    {
        // Arrange
        var specialty = CreateSpecialty();
        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1 = CreateUser();
        var user2 = CreateUser();

        Context.Users.AddRange(user1, user2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor1 = Doctor.Create(
            user1.Id,
            PersonName.Create("Doctor Same"),
            MedicalLicenseNumber.Create("CMP-11111"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(1, "Room 1", 1)
        );

        var doctor2 = Doctor.Create(
            user2.Id,
            PersonName.Create("Doctor Same"),
            MedicalLicenseNumber.Create("CMP-22222"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(2, "Room 2", 1)
        );

        Context.Doctors.AddRange(doctor1, doctor2);
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

        items.Should().HaveCount(2);
        items.Should().BeInAscendingOrder(d => d.Id);
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

        dbResult.Should().NotBeNull();
        dbResult.Id.Should().Be(doctor.Id);
        dbResult.UserId.Should().Be(user.Id);
        dbResult.FullName.Should().Be(doctor.FullName);
        dbResult.MedicalSpecialtyId.Should().Be(specialty.Id);
        dbResult.LicenseNumber.Value.Should().Be(doctor.LicenseNumber.Value);
        dbResult.Biography.Should().Be(doctor.Biography);
        dbResult.ConsultationRoom.Should().Be(doctor.ConsultationRoom);
        dbResult.IsDeleted.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(user.Id);
        result.FullName.Should().Be(doctor.FullName);
        result.MedicalSpecialtyId.Should().Be(specialty.Id);
        result.LicenseNumber.Value.Should().Be(doctor.LicenseNumber.Value);
        result.Biography.Should().Be(doctor.Biography);
        result.ConsultationRoom.Should().Be(doctor.ConsultationRoom);
        result.IsDeleted.Should().BeFalse();
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
        result.Should().NotBeNull();
        result.Id.Should().Be(doctor.Id);
        result.UserId.Should().Be(user.Id);
        result.FullName.Should().Be(doctor.FullName);
        result.MedicalSpecialtyId.Should().Be(specialty.Id);
        result.LicenseNumber.Value.Should().Be(doctor.LicenseNumber.Value);
        result.Biography.Should().Be(doctor.Biography);
        result.ConsultationRoom.Should().Be(doctor.ConsultationRoom);
        result.IsDeleted.Should().BeTrue();
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

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Doctor
        );

    private static MedicalSpecialty CreateSpecialty(string name = "Specialty") =>
        MedicalSpecialty.Create(name, "Description", 30, 24);

    private async Task<(User User, MedicalSpecialty Specialty)> CreatePrerequisitesAsync()
    {
        var user = CreateUser();
        var specialty = CreateSpecialty();

        Context.Users.Add(user);
        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        return (user, specialty);
    }
}
