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

public class UserRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly UserRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddUserToContext()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("new.user@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("+15550000001"),
            UserRole.Patient
        );

        // Act
        await _sut.CreateAsync(user, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(
                storedUser => storedUser.Id == user.Id,
                TestContext.Current.CancellationToken
            );

        dbResult.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = await CreateUserAsync();

        // Act
        var result = await _sut.GetByIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(user);
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
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailMatchesIgnoringCase()
    {
        // Arrange
        var user = await CreateUserAsync("test@clinic.com");

        // Act
        var result = await _sut.GetByEmailAsync(
            "TEST@CLINIC.COM",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        await CreateUserAsync();

        // Act
        var result = await _sut.GetByEmailAsync(
            "missing.email@clinic.com",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        await CreateUserAsync("unique.email@clinic.com");

        // Act
        var result = await _sut.ExistsByEmailAsync(
            "UNIQUE.EMAIL@CLINIC.COM",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        // Arrange
        await CreateUserAsync("existing.email@clinic.com");

        // Act
        var result = await _sut.ExistsByEmailAsync(
            "missing.email@clinic.com",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByPhoneNumberAsync_ShouldReturnTrue_WhenPhoneNumberExistsIgnoringWhitespace()
    {
        // Arrange
        await CreateUserAsync(phoneNumber: "+15550000007");

        // Act
        var result = await _sut.ExistsByPhoneNumberAsync(
            "  +15550000007  ",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByPhoneNumberAsync_ShouldReturnFalse_WhenPhoneNumberDoesNotExist()
    {
        // Arrange
        await CreateUserAsync(phoneNumber: "+15550000008");

        // Act
        var result = await _sut.ExistsByPhoneNumberAsync(
            "+15550000009",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldFilterByRole()
    {
        // Arrange
        var admin = await CreateUserAsync(UserRole.Admin);
        await CreateUserAsync(UserRole.Patient);

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            UserRole.Admin,
            null,
            null,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(admin);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldFilterByActiveStatus()
    {
        // Arrange
        var active = await CreateUserAsync("test@clinic.com");
        var inactive = await CreateUserAsync("test2@clinic.com");
        inactive.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            null,
            isActive: true,
            null,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(active);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldFilterByEmailSearchTerm()
    {
        // Arrange
        var matchingUser = await CreateUserAsync("searchable.email@clinic.com");
        await CreateUserAsync("other.email@clinic.com");

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            null,
            null,
            "SEARCHABLE.EMAIL",
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(matchingUser);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldFilterByPhoneSearchTerm()
    {
        // Arrange
        var matchingUser = await CreateUserAsync("searchable.phone@clinic.com", "+15551112222");
        await CreateUserAsync("other.phone@clinic.com", "+15553334444");

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            null,
            null,
            "1122",
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(matchingUser);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnOrderedPageAndTotalCount()
    {
        // Arrange
        var first = await CreateUserAsync("first@clinic.com", "+15550000016");
        var second = await CreateUserAsync("second@clinic.com", "+15550000017");
        await CreateUserAsync("third@clinic.com", "+15550000018");

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 2,
            null,
            null,
            null,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([first, second], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnEmpty_WhenNoUsersMatch()
    {
        // Arrange
        await CreateUserAsync(); // UserRole.Patient

        // Act
        var (items, totalCount) = await _sut.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 10,
            UserRole.Doctor,
            null,
            null,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLockedOutUsersPaginatedAsync_ShouldReturnUsersWithActiveLockout()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().DateTime;
        var lockedUser = await CreateLockedOutUserAsync(referenceTime);

        // Act
        var (items, totalCount) = await _sut.GetLockedOutUsersPaginatedAsync(
            referenceTime,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(lockedUser);
    }

    [Fact]
    public async Task GetLockedOutUsersPaginatedAsync_ShouldExcludeExpiredLockouts()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().DateTime;

        await CreateLockedOutUserAsync(referenceTime.AddMinutes(-15));
        await CreateLockedOutUserAsync(referenceTime.AddHours(-1));

        // Act
        var (items, totalCount) = await _sut.GetLockedOutUsersPaginatedAsync(
            referenceTime,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLockedOutUsersPaginatedAsync_ShouldReturnOrderedPageAndTotalCount()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().DateTime;

        await CreateLockedOutUserAsync(referenceTime);

        var second = await CreateLockedOutUserAsync(referenceTime);

        await CreateLockedOutUserAsync(referenceTime);

        // Act
        var (items, totalCount) = await _sut.GetLockedOutUsersPaginatedAsync(
            referenceTime,
            pageNumber: 2,
            pageSize: 1,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(second);
    }

    [Fact]
    public async Task GetLockedOutUsersPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().DateTime;

        await CreateLockedOutUserAsync(referenceTime);
        await CreateLockedOutUserAsync(referenceTime);

        var third = await CreateLockedOutUserAsync(referenceTime);

        // Act
        var (items, totalCount) = await _sut.GetLockedOutUsersPaginatedAsync(
            referenceTime,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(third);
    }

    private async Task<User> CreateUserAsync(
        string email = "user@clinic.com",
        string phoneNumber = "+15550000000",
        UserRole role = UserRole.Patient
    )
    {
        var user = User.Create(
            EmailAddress.Create(email),
            "hashedpassword123",
            PhoneNumber.Create(phoneNumber),
            role
        );

        Context.Users.Add(user);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<User> CreateUserAsync(UserRole role)
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            role
        );

        Context.Users.Add(user);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<User> CreateLockedOutUserAsync(DateTime lockoutReferenceTime)
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Patient
        );

        user.RecordFailedLogin(lockoutReferenceTime);
        user.RecordFailedLogin(lockoutReferenceTime);
        user.RecordFailedLogin(lockoutReferenceTime);
        user.RecordFailedLogin(lockoutReferenceTime);
        user.RecordFailedLogin(lockoutReferenceTime);

        Context.Users.Add(user);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }
}
