using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Application.Users.Queries.GetLockedOutUsers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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
    public async Task Handle_ShouldReturnPaginatedList_WhenUsersAreLocked()
    {
        // Arrange
        var lockedUser = User.Create(
            EmailAddress.Create("locked@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("111-1111"),
            UserRole.Patient
        );

        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersPaginatedAsync(
                    It.IsAny<DateTime>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<User> { lockedUser }, 1));

        // Act
        var result = await _sut.Handle(
            new GetLockedOutUsersQuery(1, 10),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.Items.Should().ContainSingle();

        var dto = result.Items.First();
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
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoUsersAreLocked()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersPaginatedAsync(
                    It.IsAny<DateTime>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<User>(), 0));

        // Act
        var result = await _sut.Handle(
            new GetLockedOutUsersQuery(1, 10),
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectReferenceTimeToRepository()
    {
        // Arrange
        var now = DateTimeOffset.Parse("2026-01-15T08:30:00Z", CultureInfo.InvariantCulture);

        _fakeTime.SetUtcNow(now);

        _userRepositoryMock
            .Setup(x =>
                x.GetLockedOutUsersPaginatedAsync(
                    It.IsAny<DateTime>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<User>(), 0));

        // Act
        await _sut.Handle(new GetLockedOutUsersQuery(1, 10), TestContext.Current.CancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            x =>
                x.GetLockedOutUsersPaginatedAsync(
                    now.UtcDateTime,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
