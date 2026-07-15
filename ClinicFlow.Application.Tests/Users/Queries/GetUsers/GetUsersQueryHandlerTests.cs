using AwesomeAssertions;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Application.Users.Queries.GetUsers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.GetUsers;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetUsersQueryHandler _sut;

    public GetUsersQueryHandlerTests()
    {
        _sut = new GetUsersQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenUsersExist()
    {
        // Arrange
        var users = new List<User>
        {
            CreateUser("user1@clinic.com", "111-1111", UserRole.Patient),
            CreateUser("user2@clinic.com", "222-2222", UserRole.Doctor),
        };

        var query = new GetUsersQuery(1, 10, null, null, null);

        _userRepositoryMock
            .Setup(x => x.GetPaginatedAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 2));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = users.Select(user => new UserDto(
            user.Id,
            user.Email.Value,
            user.PhoneNumber.Value,
            user.Role,
            user.IsActive,
            user.IsPhoneVerified,
            user.LastLoginAt,
            user.FailedLoginAttempts,
            user.LockoutEnd
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _userRepositoryMock.Verify(
            x => x.GetPaginatedAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsersMatch()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x =>
                x.GetPaginatedAsync(
                    1,
                    10,
                    UserRole.Admin,
                    true,
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<User>(), 0));

        var query = new GetUsersQuery(1, 10, UserRole.Admin, true, null);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _userRepositoryMock.Verify(
            x =>
                x.GetPaginatedAsync(
                    1,
                    10,
                    UserRole.Admin,
                    true,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static User CreateUser(string email, string phone, UserRole role) =>
        User.Create(
            EmailAddress.Create(email),
            "hashedpassword123",
            PhoneNumber.Create(phone),
            role
        );
}
