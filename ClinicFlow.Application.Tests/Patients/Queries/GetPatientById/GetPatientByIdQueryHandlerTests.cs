using ClinicFlow.Application.Patients.Queries.GetPatientById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly GetPatientByIdQueryHandler _sut;

    public GetPatientByIdQueryHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _sut = new GetPatientByIdQueryHandler(_patientRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPatient_WhenPatientExists()
    {
        // Arrange
        var patient = Patient.CreateSelf(Guid.NewGuid(), PersonName.Create("John Doe"), DateTime.UtcNow.AddYears(-30), BloodType.Create("A+"), "None", "None",
            EmergencyContact.Create("Jane", "555-1234"));
        var patientId = patient.Id;

        _patientRepositoryMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(patientId);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPatientDoesNotExist()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _patientRepositoryMock.Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);

        var query = new GetPatientByIdQuery(patientId);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage(DomainErrors.General.NotFound);
    }
}
