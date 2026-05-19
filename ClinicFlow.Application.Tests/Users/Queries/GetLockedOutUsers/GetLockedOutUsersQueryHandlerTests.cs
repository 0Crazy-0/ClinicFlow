using ClinicFlow.Application.Users.Queries.GetLockedOutUsers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.GetLockedOutUsers;

public class GetLockedOutUsersQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetLockedOutUsersQueryHandler _sut;

    public GetLockedOutUsersQueryHandlerTests()
    {
        _sut = new GetLockedOutUsersQueryHandler(_fakeTime, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnLockedOutUsers_WhenUsersAreLocked()
    {
        var lockedUser = User.Create(
            EmailAddress.Create("locked@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("111-1111"),
            UserRole.Patient
        );

        var users = new List<User> { lockedUser };

        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(users);

        // Act
        var result = await _sut.Handle(new GetLockedOutUsersQuery(), CancellationToken.None);

        // Assert
        var dto = result.First();

        dto.Id.Should().Be(lockedUser.Id);
        dto.Email.Should().Be(lockedUser.Email.Value);
        dto.PhoneNumber.Should().Be(lockedUser.PhoneNumber.Value);
        dto.Role.Should().Be(lockedUser.Role);
        dto.IsActive.Should().Be(lockedUser.IsActive);
        dto.IsPhoneVerified.Should().Be(lockedUser.IsPhoneVerified);
        dto.LastLoginAt.Should().Be(lockedUser.LastLoginAt);
        dto.FailedLoginAttempts.Should().Be(lockedUser.FailedLoginAttempts);
        dto.LockoutEnd.Should().Be(lockedUser.LockoutEnd);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoUsersAreLocked()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(new GetLockedOutUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectReferenceTimeToRepository()
    {
        // Arrange
        var now = DateTimeOffset.Parse("2026-01-15T08:30:00Z");
        _fakeTime.SetUtcNow(now);

        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(new GetLockedOutUsersQuery(), CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            x => x.GetLockedOutUsersAsync(now.UtcDateTime, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
