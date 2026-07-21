using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class ScheduleRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly ScheduleRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddScheduleToContext()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        // Act
        await _sut.CreateAsync(schedule, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .Schedules.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == schedule.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(schedule);
    }

    [Fact]
    public async Task CreateRangeAsync_ShouldAddMultipleSchedulesToContext()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        var schedule1 = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        var schedule2 = Schedule.Create(
            doctor.Id,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        // Act
        await _sut.CreateRangeAsync([schedule1, schedule2], TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResults = await Context
            .Schedules.AsNoTracking()
            .Where(s => s.DoctorId == doctor.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        dbResults.Should().BeEquivalentTo([schedule1, schedule2]);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSchedule_WhenExists()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(schedule.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(schedule);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSchedule_WhenExistsAndInactive()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        schedule.Deactivate();

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(schedule.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(schedule);
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
    public async Task GetActiveByDoctorAndDayAsync_ShouldReturnSchedule_WhenExistsAndActive()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByDoctorAndDayAsync(
            doctor.Id,
            DayOfWeek.Monday,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(schedule);
    }

    [Fact]
    public async Task GetActiveByDoctorAndDayAsync_ShouldReturnNull_WhenInactive()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        schedule.Deactivate();

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByDoctorAndDayAsync(
            doctor.Id,
            DayOfWeek.Monday,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByDoctorAndDayAsync_ShouldReturnNull_WhenWrongDoctor()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var otherDoctor = await CreateDoctorAsync();

        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByDoctorAndDayAsync(
            otherDoctor.Id,
            DayOfWeek.Monday,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByDoctorAndDayAsync_ShouldReturnNull_WhenWrongDay()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByDoctorAndDayAsync(
            doctor.Id,
            DayOfWeek.Wednesday,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByDoctorAndDayAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        // Act
        var result = await _sut.GetActiveByDoctorAndDayAsync(
            doctor.Id,
            DayOfWeek.Monday,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDoctorIdAsync_ShouldReturnAllSchedules_ForDoctor()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule1 = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(schedule1);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var schedule2 = Schedule.Create(
            doctor.Id,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        schedule2.Deactivate();

        Context.Schedules.Add(schedule2);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results
            .Should()
            .BeEquivalentTo(
                [schedule1, schedule2],
                options => options.Excluding(s => s.DomainEvents)
            );
    }

    [Fact]
    public async Task GetByDoctorIdAsync_ShouldReturnEmpty_WhenDoctorHasNoSchedules()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        // Act
        var results = await _sut.GetByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDoctorIdAsync_ShouldReturnSchedulesOrderedByDayOfWeek()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var scheduleWednesday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(scheduleWednesday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var scheduleMonday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Schedules.Add(scheduleMonday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var scheduleFriday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Friday,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        Context.Schedules.Add(scheduleFriday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results
            .Should()
            .BeEquivalentTo(
                [scheduleFriday, scheduleMonday, scheduleWednesday],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetActiveByDoctorIdAsync_ShouldReturnOnlyActiveSchedules_ForDoctor()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var active = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(active);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var inactive = Schedule.Create(
            doctor.Id,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        inactive.Deactivate();

        Context.Schedules.Add(inactive);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetActiveByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(active);
    }

    [Fact]
    public async Task GetActiveByDoctorIdAsync_ShouldReturnEmpty_WhenAllSchedulesInactive()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        var schedule = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        schedule.Deactivate();

        Context.Schedules.Add(schedule);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetActiveByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveByDoctorIdAsync_ShouldReturnEmpty_WhenDoctorHasNoSchedules()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        // Act
        var results = await _sut.GetActiveByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveByDoctorIdAsync_ShouldReturnSchedulesOrderedByDayOfWeek()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        var scheduleWednesday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        Context.Schedules.Add(scheduleWednesday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var scheduleMonday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        Context.Schedules.Add(scheduleMonday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var scheduleFriday = Schedule.Create(
            doctor.Id,
            DayOfWeek.Friday,
            TimeRange.Create(new TimeOnly(11, 0), new TimeOnly(12, 0))
        );

        Context.Schedules.Add(scheduleFriday);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetActiveByDoctorIdAsync(
            doctor.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        results
            .Should()
            .BeEquivalentTo(
                [scheduleFriday, scheduleMonday, scheduleWednesday],
                options => options.WithStrictOrdering()
            );
    }

    private async Task<Doctor> CreateDoctorAsync()
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.NewGuid()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Doctor
        );

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync();

        var roomNumber = Random.Shared.Next(
            ConsultationRoom.MinimumNumber,
            ConsultationRoom.MaximumNumber + 1
        );

        var floor = Random.Shared.Next(
            ConsultationRoom.MinimumFloor,
            ConsultationRoom.MaximumFloor + 1
        );

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
}
