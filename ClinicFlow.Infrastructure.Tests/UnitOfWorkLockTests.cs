using AwesomeAssertions;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Tests.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicFlow.Infrastructure.Tests;

public class UnitOfWorkLockTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly UnitOfWork _sut;
    private ApplicationDbContext Context => _fixture.Context;

    public UnitOfWorkLockTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _publisherMock = new Mock<IPublisher>();
        _sut = new UnitOfWork(fixture.Context, _publisherMock.Object);
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
    public async Task ExecuteWithLockAsync_ShouldExecuteOperationAndReturnResult_WithSingleGuidLockKey()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();

        // Act
        var result = await _sut.ExecuteWithLockAsync(
            lockKey,
            _ => Task.FromResult("operation_completed"),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().Be("operation_completed");
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldExecuteOperationAndReturnResult_WithDualLockKey()
    {
        // Arrange
        var lockKeyPart1 = Guid.CreateVersion7();

        // Act
        var result = await _sut.ExecuteWithLockAsync(
            lockKeyPart1,
            42L,
            _ => Task.FromResult(100),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldCommitDatabaseChanges_WhenOperationSucceeds()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();
        var user = CreateUser();

        // Act
        await _sut.ExecuteWithLockAsync(
            lockKey,
            async cancellationToken =>
            {
                Context.Users.Add(user);
                return await _sut.SaveChangesAsync(cancellationToken);
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        var dbUser = await Context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);

        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldRollbackDatabaseChanges_WhenOperationThrowsException()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();
        var user = CreateUser();

        // Act
        var act = () =>
            _sut.ExecuteWithLockAsync<int>(
                lockKey,
                async cancellationToken =>
                {
                    Context.Users.Add(user);
                    await _sut.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException();
                },
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        var dbUser = await Context
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);

        dbUser.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldDeferDomainEvents_UntilTransactionCommits()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();
        var user = CreateUser();

        var domainEvent = new TestDomainEvent();
        user.AddDomainEvent(domainEvent);

        Context.Users.Add(user);

        var wasPublishedDuringSave = false;

        // Act
        await _sut.ExecuteWithLockAsync(
            lockKey,
            async cancellationToken =>
            {
                await _sut.SaveChangesAsync(cancellationToken);

                wasPublishedDuringSave = _publisherMock.Invocations.Count > 0;

                return true;
            },
            TestContext.Current.CancellationToken
        );

        // Assert
        wasPublishedDuringSave.Should().BeFalse();

        _publisherMock.Verify(
            x =>
                x.Publish(
                    It.Is<INotification>(n =>
                        n is DomainEventNotification<TestDomainEvent>
                        && ((DomainEventNotification<TestDomainEvent>)n).DomainEvent == domainEvent
                    ),
                    TestContext.Current.CancellationToken
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldNotPublishDomainEvents_WhenOperationFails()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();
        var user = CreateUser();

        var domainEvent = new TestDomainEvent();
        user.AddDomainEvent(domainEvent);

        Context.Users.Add(user);

        // Act
        var act = () =>
            _sut.ExecuteWithLockAsync<bool>(
                lockKey,
                async cancellationToken =>
                {
                    await _sut.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException();
                },
                TestContext.Current.CancellationToken
            );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldClearLeftoverPendingNotifications_FromPreviousFailedOperations()
    {
        // Arrange
        var lockKey1 = Guid.CreateVersion7();
        var lockKey2 = Guid.CreateVersion7();
        var user1 = CreateUser();

        var domainEvent1 = new TestDomainEvent();
        user1.AddDomainEvent(domainEvent1);

        Context.Users.Add(user1);

        var failedAct = () =>
            _sut.ExecuteWithLockAsync<bool>(
                lockKey1,
                async cancellationToken =>
                {
                    await _sut.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException();
                },
                TestContext.Current.CancellationToken
            );

        await failedAct.Should().ThrowAsync<InvalidOperationException>();

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        // Act
        await _sut.ExecuteWithLockAsync(
            lockKey2,
            _ => Task.FromResult(true),
            TestContext.Current.CancellationToken
        );

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldClearPendingNotifications_AfterPublishing()
    {
        // Arrange
        var lockKey1 = Guid.CreateVersion7();
        var lockKey2 = Guid.CreateVersion7();
        var user = CreateUser();

        var domainEvent = new TestDomainEvent();
        user.AddDomainEvent(domainEvent);

        Context.Users.Add(user);

        await _sut.ExecuteWithLockAsync(
            lockKey1,
            async cancellationToken =>
            {
                await _sut.SaveChangesAsync(cancellationToken);
                return true;
            },
            TestContext.Current.CancellationToken
        );

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Act
        await _sut.ExecuteWithLockAsync(
            lockKey2,
            _ => Task.FromResult(true),
            TestContext.Current.CancellationToken
        );

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishDomainEventsImmediately_WhenNoActiveTransaction()
    {
        // Arrange
        var user = CreateUser();

        var domainEvent = new TestDomainEvent();
        user.AddDomainEvent(domainEvent);

        Context.Users.Add(user);

        // Act
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _publisherMock.Verify(
            x =>
                x.Publish(
                    It.Is<INotification>(n =>
                        n is DomainEventNotification<TestDomainEvent>
                        && ((DomainEventNotification<TestDomainEvent>)n).DomainEvent == domainEvent
                    ),
                    TestContext.Current.CancellationToken
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldBlockConcurrentExecution_WhenSameLockKeyIsUsed()
    {
        // Arrange
        var lockKey = Guid.CreateVersion7();

        using var secondaryContext = CreateSecondaryDbContext();

        var secondaryPublisherMock = new Mock<IPublisher>();
        var secondarySut = new UnitOfWork(secondaryContext, secondaryPublisherMock.Object);

        var task1Started = new TaskCompletionSource();
        var task1Release = new TaskCompletionSource();
        var task2Executed = false;

        // Act
        var task1 = _sut.ExecuteWithLockAsync(
            lockKey,
            async cancellationToken =>
            {
                task1Started.SetResult();
                await task1Release.Task;
                return true;
            },
            TestContext.Current.CancellationToken
        );

        await task1Started.Task;

        var task2 = Task.Run(
            async () =>
            {
                await secondarySut.ExecuteWithLockAsync(
                    lockKey,
                    _ =>
                    {
                        task2Executed = true;
                        return Task.FromResult(true);
                    },
                    TestContext.Current.CancellationToken
                );
            },
            TestContext.Current.CancellationToken
        );

        await Task.Delay(150, TestContext.Current.CancellationToken);

        // Assert
        task2Executed.Should().BeFalse();

        task1Release.SetResult();
        await task1;
        await task2;

        task2Executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithLockAsync_ShouldAllowConcurrentExecution_WhenDifferentLockKeysAreUsed()
    {
        // Arrange
        var lockKey1 = Guid.CreateVersion7();
        var lockKey2 = Guid.CreateVersion7();

        using var secondaryContext = CreateSecondaryDbContext();

        var secondaryPublisherMock = new Mock<IPublisher>();
        var secondarySut = new UnitOfWork(secondaryContext, secondaryPublisherMock.Object);

        var task1Started = new TaskCompletionSource();
        var task1Release = new TaskCompletionSource();
        var task2Executed = false;

        var task1 = _sut.ExecuteWithLockAsync(
            lockKey1,
            async cancellationToken =>
            {
                task1Started.SetResult();
                await task1Release.Task;
                return true;
            },
            TestContext.Current.CancellationToken
        );

        await task1Started.Task;

        // Act
        var task2 = secondarySut.ExecuteWithLockAsync(
            lockKey2,
            _ =>
            {
                task2Executed = true;
                return Task.FromResult(true);
            },
            TestContext.Current.CancellationToken
        );

        await task2;

        // Assert
        task2Executed.Should().BeTrue();

        task1Release.SetResult();
        await task1;
    }

    private ApplicationDbContext CreateSecondaryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_fixture.Context.Database.GetConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateUser(
        string email = "test@clinic.com",
        string phoneNumber = "+15550000000"
    ) =>
        User.Create(
            EmailAddress.Create(email),
            "hashed_password",
            PhoneNumber.Create(phoneNumber),
            UserRole.Patient
        );

    private class TestDomainEvent : IDomainEvent { }
}
