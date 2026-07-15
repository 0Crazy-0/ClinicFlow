using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordByIdQueryHandler _sut;

    public GetMedicalRecordByIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordByIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMedicalRecordDto_WhenRecordExists()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var request = new GetMedicalRecordByIdQuery(id);
        var record = MedicalRecord.Create(patientId, doctorId, appointmentId, "Headache");
        var clinicalDetail = DynamicClinicalDetail.Create("vital-signs", "{}");

        record.AddClinicalDetail(clinicalDetail);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(id, TestContext.Current.CancellationToken))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new MedicalRecordDto(
            record.Id,
            record.PatientId,
            record.DoctorId,
            record.AppointmentId,
            record.ChiefComplaint,
            [
                .. record.ClinicalDetails.Select(d => new ClinicalDetailDto(
                    d.TemplateCode,
                    d.JsonDataPayload
                )),
            ]
        );

        result.Should().BeEquivalentTo(expectedDto);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(id, TestContext.Current.CancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenRecordDoesNotExist()
    {
        // Arrange
        var request = new GetMedicalRecordByIdQuery(Guid.CreateVersion7());

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = async () => await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
