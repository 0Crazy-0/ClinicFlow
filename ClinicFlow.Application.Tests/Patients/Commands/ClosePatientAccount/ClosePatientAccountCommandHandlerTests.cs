using ClinicFlow.Application.Patients.Commands.ClosePatientAccount;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
        _sut = new ClosePatientAccountCommandHandler(
            _patientRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCloseAccountAndRemoveFamilyMembers_WhenNoPendingAppointments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ClosePatientAccountCommand(userId);

        var primaryPatient = Patient.CreateSelf(
            userId,
            PersonName.Create("Primary User"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var familyMember = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child User"),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-5).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patients = new List<Patient> { primaryPatient, familyMember };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        primaryPatient.IsDeleted.Should().BeTrue();
        familyMember.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCloseAccountSuccessfully_WhenNoFamilyMembers()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ClosePatientAccountCommand(userId);

        var primaryPatient = Patient.CreateSelf(
            userId,
            PersonName.Create("Primary User"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patients = new List<Patient> { primaryPatient };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        primaryPatient.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenHasPendingAppointments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ClosePatientAccountCommand(userId);

        var primaryPatient = Patient.CreateSelf(
            userId,
            PersonName.Create("Primary User"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patients = new List<Patient> { primaryPatient };

        _patientRepositoryMock
            .Setup(x => x.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patients);

        _appointmentRepositoryMock
            .Setup(x => x.HasActiveAppointmentsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotCloseAccountWithPendingAppointments);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
