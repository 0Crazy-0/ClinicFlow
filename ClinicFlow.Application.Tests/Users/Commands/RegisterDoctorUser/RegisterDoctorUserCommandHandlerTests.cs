using AwesomeAssertions;
using ClinicFlow.Application.Users.Commands.RegisterDoctorUser;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.RegisterDoctorUser;

public class RegisterDoctorUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RegisterDoctorUserCommandHandler _sut;

    public RegisterDoctorUserCommandHandlerTests()
    {
        _sut = new RegisterDoctorUserCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateUserAsDoctor_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("doctor@clinic.com", "password123", "555-1234");

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherServiceMock.Setup(x => x.Hash(command.Password)).Returns("hashed_password");

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        capturedUser.Should().NotBeNull();
        capturedUser.Email.Value.Should().Be(command.Email);
        capturedUser.PasswordHash.Should().Be("hashed_password");
        capturedUser.PhoneNumber.Value.Should().Be(command.PhoneNumber);
        capturedUser.Role.Should().Be(UserRole.Doctor);
        capturedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("doctor@clinic.com", "password123", "555-1234");

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherServiceMock.Setup(x => x.Hash(command.Password)).Returns("hashed_password");

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterDoctorUserCommand("doctor@clinic.com", "password123", "555-1234");

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.EmailAlreadyExists);

        _userRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
