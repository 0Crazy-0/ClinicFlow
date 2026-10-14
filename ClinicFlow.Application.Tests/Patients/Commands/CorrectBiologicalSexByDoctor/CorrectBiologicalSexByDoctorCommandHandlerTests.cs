using AwesomeAssertions;
using ClinicFlow.Application.Patients.Commands.CorrectBiologicalSexByDoctor;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.CorrectBiologicalSexByDoctor;

public class CorrectBiologicalSexByDoctorCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CorrectBiologicalSexByDoctorCommandHandler _sut;

    public CorrectBiologicalSexByDoctorCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new CorrectBiologicalSexByDoctorCommandHandler(
            _fakeTime,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData(BiologicalSex.Male, BiologicalSex.Female)]
    [InlineData(BiologicalSex.Female, BiologicalSex.Male)]
    public async Task Handle_ShouldCorrectSexAndSaveChanges_WhenDoctorCorrectsToDifferentSex(
        BiologicalSex currentSex,
        BiologicalSex correctedSex
    )
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var doctor = CreateDoctor(initiatorUserId);
        var patient = CreatePatient(currentSex);
        var reason = "reason";

        var request = new CorrectBiologicalSexByDoctorCommand(
            patient.Id,
            initiatorUserId,
            correctedSex,
            reason
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        patient.SexAtBirth.Should().Be(correctedSex);
        patient.DomainEvents.OfType<PatientBiologicalSexCorrectedEvent>().Should().ContainSingle();

        var evt = patient.DomainEvents.OfType<PatientBiologicalSexCorrectedEvent>().Single();

        evt.PatientId.Should().Be(patient.Id);
        evt.PreviousSex.Should().Be(currentSex);
        evt.CorrectedSex.Should().Be(correctedSex);
        evt.CorrectedByDoctorId.Should().Be(doctor.Id);
        evt.Reason.Should().Be(reason);

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var request = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            BiologicalSex.Female,
            "reason"
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(request.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(request.PatientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorHasNoDoctorProfile()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patient = CreatePatient(BiologicalSex.Male);
        var request = new CorrectBiologicalSexByDoctorCommand(
            patient.Id,
            initiatorUserId,
            BiologicalSex.Female,
            "reason"
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

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

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessRuleValidationException_WhenCorrectingToSameSex()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var doctor = CreateDoctor(initiatorUserId);
        var patient = CreatePatient(BiologicalSex.Male);
        var request = new CorrectBiologicalSexByDoctorCommand(
            patient.Id,
            initiatorUserId,
            BiologicalSex.Male,
            "No change"
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _doctorRepositoryMock
            .Setup(x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Patient.BiologicalSexUnchanged);

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _doctorRepositoryMock.Verify(
            x => x.GetByUserIdAsync(initiatorUserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Patient CreatePatient(BiologicalSex biologicalSex) =>
        Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            biologicalSex,
            _fakeTime.GetUtcNow().UtcDateTime
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
}
