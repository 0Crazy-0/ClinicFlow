using AwesomeAssertions;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Application.Users.Queries.GetUserById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.GetUserById;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _sut = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserExists()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(
            new GetUserByIdQuery(user.Id),
            TestContext.Current.CancellationToken
        );

        // Assert
        var expectedDto = new UserDto(
            user.Id,
            user.Email.Value,
            user.PhoneNumber.Value,
            user.Role,
            user.IsActive,
            user.IsPhoneVerified,
            user.LastLoginAt,
            user.FailedLoginAttempts,
            user.LockoutEnd
        );

        result.Should().BeEquivalentTo(expectedDto);

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () =>
            await _sut.Handle(new GetUserByIdQuery(userId), TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
