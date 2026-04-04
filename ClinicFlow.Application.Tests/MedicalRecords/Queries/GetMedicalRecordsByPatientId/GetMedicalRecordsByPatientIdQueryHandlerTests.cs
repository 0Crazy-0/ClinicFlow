using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public class GetMedicalRecordsByPatientIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordsByPatientIdQueryHandler _sut;

    public GetMedicalRecordsByPatientIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordsByPatientIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMedicalRecordDtos_WhenRecordsExistForPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId1 = Guid.NewGuid();
        var doctorId2 = Guid.NewGuid();
        var request = new GetMedicalRecordsByPatientIdQuery(patientId);

        var record1 = CreateMedicalRecord(patientId, doctorId1, Guid.NewGuid(), "Checkup");
        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        var record2 = CreateMedicalRecord(patientId, doctorId2, Guid.NewGuid(), "Follow-up");

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId, CancellationToken.None))
            .ReturnsAsync([record1, record2]);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstResult = result.First(r => r.Id == record1.Id);
        firstResult.PatientId.Should().Be(patientId);
        firstResult.DoctorId.Should().Be(doctorId1);
        firstResult.ChiefComplaint.Should().Be("Checkup");
        firstResult.ClinicalDetails.Should().ContainSingle();

        var secondResult = result.First(r => r.Id == record2.Id);
        secondResult.PatientId.Should().Be(patientId);
        secondResult.DoctorId.Should().Be(doctorId2);
        secondResult.ChiefComplaint.Should().Be("Follow-up");
        secondResult.ClinicalDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRecordsExistForPatient()
    {
        // Arrange
        var request = new GetMedicalRecordsByPatientIdQuery(Guid.NewGuid());

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(request.PatientId, CancellationToken.None))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private static MedicalRecord CreateMedicalRecord(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    ) => MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);
}
