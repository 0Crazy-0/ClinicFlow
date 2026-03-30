using ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Commands.UpdatePatientProfile;

public class UpdatePatientProfileCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly UpdatePatientProfileCommandHandler _sut;

    public UpdatePatientProfileCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new UpdatePatientProfileCommandHandler(
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateProfile_WhenPatientExists()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.NewGuid(),
            "O+",
            "Peanuts",
            "Asthma",
            "Dad",
            "555-1234"
        );
        var patient = Patient.CreateSelf(
            command.PatientId,
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _patientRepositoryMock.Verify(
            x => x.UpdateAsync(patient, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        patient.BloodType.ToString().Should().Be(command.BloodType);
        patient.Allergies.Should().Be(command.Allergies);
        patient.ChronicConditions.Should().Be(command.ChronicConditions);
        patient.EmergencyContact.Name.ToString().Should().Be(command.EmergencyContactName);
        patient.EmergencyContact.PhoneNumber.ToString().Should().Be(command.EmergencyContactPhone);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientDoesNotExist()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.NewGuid(),
            "O+",
            "Peanuts",
            "Asthma",
            "Dad",
            "555-1234"
        );

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _patientRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
