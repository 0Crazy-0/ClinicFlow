using AwesomeAssertions;
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
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        var items = result.Items.ToList();
        items[0].Role.Should().Be(UserRole.Patient);
        items[1].Role.Should().Be(UserRole.Doctor);
        items.Should().HaveCount(2);
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
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldMapUserPropertiesCorrectly()
    {
        // Arrange
        var user = CreateUser("admin@clinic.com", "333-3333", UserRole.Admin);
        var users = new List<User> { user };

        _userRepositoryMock
            .Setup(x => x.GetPaginatedAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 1));

        var query = new GetUsersQuery(1, 10, null, null, null);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var dto = result.Items.First();
        dto.Id.Should().Be(user.Id);
        dto.Email.Should().Be(user.Email.Value);
        dto.PhoneNumber.Should().Be(user.PhoneNumber.Value);
        dto.Role.Should().Be(user.Role);
        dto.IsActive.Should().Be(user.IsActive);
        dto.IsPhoneVerified.Should().Be(user.IsPhoneVerified);
    }

    private static User CreateUser(string email, string phone, UserRole role) =>
        User.Create(
            EmailAddress.Create(email),
            "hashedpassword123",
            PhoneNumber.Create(phone),
            role
        );
}
