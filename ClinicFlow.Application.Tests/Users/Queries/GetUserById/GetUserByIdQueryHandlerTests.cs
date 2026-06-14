using AwesomeAssertions;
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
        var result = await _sut.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.PhoneNumber.Should().Be(user.PhoneNumber.Value);
        result.Role.Should().Be(user.Role);
        result.IsActive.Should().Be(user.IsActive);
        result.IsPhoneVerified.Should().Be(user.IsPhoneVerified);
        result.LastLoginAt.Should().BeNull();
        result.FailedLoginAttempts.Should().Be(0);
        result.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () =>
            await _sut.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));
    }
}
