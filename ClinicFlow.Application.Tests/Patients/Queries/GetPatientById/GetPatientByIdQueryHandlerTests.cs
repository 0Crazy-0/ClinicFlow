using ClinicFlow.Application.Patients.Queries.GetPatientById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
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
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(patientId);
        result.FullName.Should().Be("John Doe");
        result.BloodType.Should().Be("A+");
        result.EmergencyContactName.Should().Be("Jane");
        result.EmergencyContactPhone.Should().Be("555-1234");
    }

    [Fact]
    public async Task Handle_ShouldReturnPatient_WhenProfileIsIncomplete()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var patientId = patient.Id;

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(patientId);
        result.FullName.Should().Be("John Doe");
        result.BloodType.Should().BeNull();
        result.Allergies.Should().Be(string.Empty);
        result.ChronicConditions.Should().Be(string.Empty);
        result.EmergencyContactName.Should().BeNull();
        result.EmergencyContactPhone.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
    }
}
