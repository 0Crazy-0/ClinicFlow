using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicFlow.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly UnitOfWork _sut;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _publisherMock = new Mock<IPublisher>();
        _sut = new UnitOfWork(_dbContext, _publisherMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChangesInDatabase()
    {
        // Arrange
        var email = EmailAddress.Create("test@clinic.com");
        var phone = PhoneNumber.Create("555-1234");
        var user = User.Create(email, "hashed_password", phone, UserRole.Patient);

        _dbContext.Users.Add(user);

        // Act
        var result = await _sut.SaveChangesAsync(CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        var dbUser = await _dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        dbUser.Should().NotBeNull();
        dbUser.Email.Should().Be(email);
        dbUser.PhoneNumber.Should().Be(phone);
        dbUser.Role.Should().Be(UserRole.Patient);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPublishDomainEvents_WhenEntitiesHaveDomainEvents()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashed_password",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        var domainEvent = new TestDomainEvent();

        user.AddDomainEvent(domainEvent);

        _dbContext.Users.Add(user);

        // Act
        await _sut.SaveChangesAsync(CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x =>
                x.Publish(
                    It.Is<INotification>(n =>
                        n is DomainEventNotification<TestDomainEvent>
                        && ((DomainEventNotification<TestDomainEvent>)n).DomainEvent == domainEvent
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEvents_AfterCapturingThem()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashed_password",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        var domainEvent = new TestDomainEvent();

        user.AddDomainEvent(domainEvent);

        _dbContext.Users.Add(user);

        // Act
        await _sut.SaveChangesAsync(CancellationToken.None);

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotPublishDomainEvents_WhenNoEntitiesHaveDomainEvents()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashed_password",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        _dbContext.Users.Add(user);

        // Act
        await _sut.SaveChangesAsync(CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldForwardCancellationTokenToPublisher()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashed_password",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        var domainEvent = new TestDomainEvent();

        user.AddDomainEvent(domainEvent);

        _dbContext.Users.Add(user);

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        await _sut.SaveChangesAsync(cancellationToken);

        // Assert
        _publisherMock.Verify(
            x =>
                x.Publish(
                    It.Is<INotification>(n =>
                        n is DomainEventNotification<TestDomainEvent>
                        && ((DomainEventNotification<TestDomainEvent>)n).DomainEvent == domainEvent
                    ),
                    cancellationToken
                ),
            Times.Once
        );
    }

    private class TestDomainEvent : IDomainEvent
    {
        public Guid EventId = Guid.NewGuid();
    }
}
