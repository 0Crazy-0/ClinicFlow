using AwesomeAssertions;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Application.Users.Queries.GetUsers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.GetUsers;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetUsersQueryHandler _sut;

    public GetUsersQueryHandlerTests()
    {
        _sut = new GetUsersQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenUsersExist()
    {
        // Arrange
        var patientUser = CreatePatientUser("user1@clinic.com", "111-1111");
        var secondPatientUser = CreatePatientUser("user2@clinic.com", "222-2222");
        var doctorUser = User.Create(
            EmailAddress.Create("doctor@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("333-3333"),
            UserRole.Doctor,
            _fakeTime.GetLocalNow().DateTime
        );

        var users = new List<User> { patientUser, secondPatientUser, doctorUser };

        var query = new GetUsersQuery(1, 10, null, null, null);

        _userRepositoryMock
            .Setup(x => x.GetPaginatedAsync(1, 10, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 3));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new[]
        {
            CreateExpectedUserDto(patientUser, UserRole.Patient),
            CreateExpectedUserDto(secondPatientUser, UserRole.Patient),
            CreateExpectedUserDto(doctorUser, UserRole.Doctor),
        };

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result
            .Items.Should()
            .ContainSingle(dto => dto.Id == doctorUser.Id)
            .Which.Role.Should()
            .Be(UserRole.Doctor);

        result.TotalCount.Should().Be(3);
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

    private User CreatePatientUser(string email, string phone)
    {
        var user = User.CreatePending(EmailAddress.Create(email), PhoneNumber.Create(phone));

        user.Activate("hashedpassword123", _fakeTime.GetLocalNow().DateTime);

        return user;
    }

    private static UserDto CreateExpectedUserDto(User user, UserRole expectedRole) =>
        new(
            user.Id,
            user.Email.Value,
            user.Phone.Value,
            expectedRole,
            user.IsActive,
            user.LastLoginAt,
            user.FailedLoginAttempts,
            user.LockoutEnd
        );
}
