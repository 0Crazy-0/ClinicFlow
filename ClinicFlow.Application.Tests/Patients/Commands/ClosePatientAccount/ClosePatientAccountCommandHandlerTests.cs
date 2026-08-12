using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.ClosePatientAccount;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.ClosePatientAccount;

public class ClosePatientAccountCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly ClosePatientAccountCommandHandler _sut;

    public ClosePatientAccountCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();

        _unitOfWorkMock
            .Setup(x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    Guid _,
                    Func<CancellationToken, Task> operation,
                    CancellationToken cancellationToken
                ) => operation(cancellationToken)
            );

        _sut = new ClosePatientAccountCommandHandler(
            _patientRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCloseAccountSuccessfully_WhenNoFamilyMembersAndNoPendingAppointments()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new ClosePatientAccountCommand(userId);

        var primaryPatient = Patient.CreateProfile(
            PersonName.Create("Primary User"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _patientRepositoryMock
            .Setup(x => x.HasActiveFamilyMembersAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _patientRepositoryMock
            .Setup(x => x.GetSelfPatientByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(primaryPatient);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        primaryPatient.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    command.UserId,
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowActiveFamilyMembersExistException_WhenActiveFamilyMembersExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new ClosePatientAccountCommand(userId);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _patientRepositoryMock
            .Setup(x => x.HasActiveFamilyMembersAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<ActiveFamilyMembersExistException>()
            .WithMessage(DomainErrors.User.CannotCloseAccountWithActiveFamilyMembers);

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenHasPendingAppointments()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new ClosePatientAccountCommand(userId);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotCloseAccountWithPendingAppointments);

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSelfPatientDoesNotExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new ClosePatientAccountCommand(userId);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _patientRepositoryMock
            .Setup(x => x.HasActiveFamilyMembersAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _patientRepositoryMock
            .Setup(x => x.GetSelfPatientByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
