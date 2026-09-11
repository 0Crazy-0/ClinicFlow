using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;

public class SetGuardianInvolvementDeterminationByDoctorCommandHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly SetGuardianInvolvementDeterminationByDoctorCommandHandler _sut;

    public SetGuardianInvolvementDeterminationByDoctorCommandHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new SetGuardianInvolvementDeterminationByDoctorCommandHandler(
            _medicalRecordRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldSetDeterminationAndSaveChanges_WhenInitiatorIsAppointmentDoctor(
        bool guardianInvolvementDeemedAppropriate
    )
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var doctor = CreateDoctor(initiatorUserId);
        var appointment = CreateInProgressAppointment(doctor.Id);
        var record = CreateMedicalRecord(appointment.Id);

        var request = new SetGuardianInvolvementDeterminationByDoctorCommand(
            record.Id,
            initiatorUserId,
            guardianInvolvementDeemedAppropriate
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        record
            .GuardianInvolvementDeemedAppropriate.Should()
            .Be(guardianInvolvementDeemedAppropriate);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMedicalRecordDoesNotExist()
    {
        // Arrange
        var request = new SetGuardianInvolvementDeterminationByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            true
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(request.MedicalRecordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(request.MedicalRecordId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var medicalRecordId = Guid.CreateVersion7();
        var request = new SetGuardianInvolvementDeterminationByDoctorCommand(
            medicalRecordId,
            Guid.CreateVersion7(),
            true
        );

        var record = CreateMedicalRecord(Guid.CreateVersion7());

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(record.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(medicalRecordId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(record.AppointmentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenInitiatorIsNotAppointmentDoctor()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var appointment = CreateInProgressAppointment(Guid.CreateVersion7());
        var record = CreateMedicalRecord(appointment.Id);
        var unrelatedDoctor = CreateDoctor(initiatorUserId);
        var request = new SetGuardianInvolvementDeterminationByDoctorCommand(
            record.Id,
            initiatorUserId,
            true
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unrelatedDoctor);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedDoctor);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorHasNoDoctorProfile()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var appointment = CreateInProgressAppointment(Guid.CreateVersion7());
        var record = CreateMedicalRecord(appointment.Id);
        var request = new SetGuardianInvolvementDeterminationByDoctorCommand(
            record.Id,
            initiatorUserId,
            true
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static MedicalRecord CreateMedicalRecord(Guid appointmentId) =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            appointmentId,
            "Headache",
            ProtectedCategory.MentalHealthCounseling,
            null
        );

    private static Doctor CreateDoctor(Guid userId) =>
        Doctor.Create(
            userId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("RM-12345"),
            Guid.CreateVersion7(),
            "Bio",
            ConsultationRoom.Create(1, "Room A", 1)
        );

    private Appointment CreateInProgressAppointment(Guid doctorId)
    {
        var now = _fakeTime.GetUtcNow().UtcDateTime;
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(now.AddDays(-1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10))
        );

        appointment.CheckIn(DateOnly.FromDateTime(now));
        appointment.Start(doctorId, now);

        return appointment;
    }
}
