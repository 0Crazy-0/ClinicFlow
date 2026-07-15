using AwesomeAssertions;
using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Application.Patients.Queries.GetPatientById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly GetPatientByIdQueryHandler _sut;

    public GetPatientByIdQueryHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _fakeTime = new FakeTimeProvider();
        _sut = new GetPatientByIdQueryHandler(_patientRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPatient_WhenPatientExists()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("A+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Jane", "555-1234"));

        var patientId = patient.Id;

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new PatientDto(
            patient.Id,
            patient.UserId,
            patient.FullName.FullName,
            patient.RelationshipToUser,
            patient.DateOfBirth,
            patient.BloodType?.Value,
            patient.Allergies,
            patient.ChronicConditions,
            patient.EmergencyContact?.Name.ToString(),
            patient.EmergencyContact?.PhoneNumber.ToString()
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPatient_WhenProfileIsIncomplete()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var patientId = patient.Id;

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new PatientDto(
            patient.Id,
            patient.UserId,
            patient.FullName.FullName,
            patient.RelationshipToUser,
            patient.DateOfBirth,
            patient.BloodType?.Value,
            patient.Allergies,
            patient.ChronicConditions,
            patient.EmergencyContact?.Name.ToString(),
            patient.EmergencyContact?.PhoneNumber.ToString()
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
