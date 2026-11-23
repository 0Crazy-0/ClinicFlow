using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class PatientActivationCodeRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly PatientActivationCodeRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddActivationCodeToContext()
    {
        // Arrange
        var user = await CreateUserAsync();
        var activationCode = PatientActivationCode.Issue(
            user.Id,
            CreateCodeHash(),
            Guid.CreateVersion7(),
            _fakeTime.GetLocalNow().DateTime
        );
        // Act
        await _sut.CreateAsync(activationCode, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .PatientActivationCodes.AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == activationCode.Id,
                TestContext.Current.CancellationToken
            );

        dbResult
            .Should()
            .BeEquivalentTo(activationCode, options => options.Excluding(c => c.DomainEvents));
    }

    [Fact]
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var user = await CreateUserAsync();
        var activationCode = PatientActivationCode.Issue(
            user.Id,
            CreateCodeHash(),
            Guid.CreateVersion7(),
            _fakeTime.GetLocalNow().DateTime
        );

        Context.PatientActivationCodes.Add(activationCode);
        Context.Entry(activationCode).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(activationCode, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(activationCode).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnActivationCode_WhenExistsAndActive()
    {
        // Arrange
        var user = await CreateUserAsync();
        var activationCode = await CreateActivationCodeAsync(user.Id);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo(activationCode, options => options.Excluding(c => c.DomainEvents));
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            nonExistentUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnNull_WhenCodeIsConsumed()
    {
        // Arrange
        var user = await CreateUserAsync();
        var activationCode = await CreateActivationCodeAsync(user.Id);

        activationCode.Consume(_fakeTime.GetLocalNow().DateTime.AddMinutes(1));

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnNull_WhenCodeIsSuperseded()
    {
        // Arrange
        var user = await CreateUserAsync();
        var activationCode = await CreateActivationCodeAsync(user.Id);

        activationCode.Supersede(_fakeTime.GetLocalNow().DateTime.AddMinutes(1));

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnNull_WhenOnlyOtherUsersHaveActiveCodes()
    {
        // Arrange
        var requestedUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        await CreateActivationCodeAsync(otherUser.Id);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            requestedUser.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnOnlyRequestedUserCode_WhenMultipleUsersHaveActiveCodes()
    {
        // Arrange
        var requestedUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        var expected = await CreateActivationCodeAsync(requestedUser.Id);

        await CreateActivationCodeAsync(otherUser.Id);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            requestedUser.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo(expected, options => options.Excluding(c => c.DomainEvents));
    }

    [Fact]
    public async Task ShouldReturnOnlyActiveCode_WhenUserAlsoHasConsumedAndSupersededCodes()
    {
        // Arrange
        // The filtered unique index on UserId allows only one active code per user,
        // so the consumed and superseded codes are closed before the next one is issued.
        var user = await CreateUserAsync();

        var consumedCode = await CreateActivationCodeAsync(user.Id);

        consumedCode.Consume(_fakeTime.GetLocalNow().DateTime.AddMinutes(1));

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var supersededCode = await CreateActivationCodeAsync(user.Id);

        supersededCode.Supersede(_fakeTime.GetLocalNow().DateTime.AddMinutes(2));

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var activeCode = await CreateActivationCodeAsync(user.Id);

        // Act
        var result = await _sut.GetActiveByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo(activeCode, options => options.Excluding(c => c.DomainEvents));
    }

    private static string CreateCodeHash() => $"{Guid.CreateVersion7():N}{Guid.CreateVersion7():N}";

    private async Task<User> CreateUserAsync()
    {
        var email = EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com");
        var phone = PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}");
        var user = User.CreatePending(email, phone);

        Context.Users.Add(user);

        await Context.SaveChangesAsync();

        return user;
    }

    private async Task<PatientActivationCode> CreateActivationCodeAsync(Guid userId)
    {
        var activationCode = PatientActivationCode.Issue(
            userId,
            CreateCodeHash(),
            Guid.CreateVersion7(),
            _fakeTime.GetLocalNow().DateTime
        );

        Context.PatientActivationCodes.Add(activationCode);

        await Context.SaveChangesAsync();

        return activationCode;
    }
}
