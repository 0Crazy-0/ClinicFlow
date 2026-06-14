using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Interfaces.Repositories;
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
    public async Task Handle_ShouldReturnPaginatedList_WhenRecordsExistForPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var request = new GetMedicalRecordsByPatientIdQuery(patientId, 1, 10);

        var record1 = CreateMedicalRecord(patientId, Guid.NewGuid(), Guid.NewGuid(), "Checkup");
        var record2 = CreateMedicalRecord(patientId, Guid.NewGuid(), Guid.NewGuid(), "Follow-up");

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([record1, record2], 2));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var firstResult = result.Items.First(r => r.Id == record1.Id);
        firstResult.PatientId.Should().Be(patientId);
        firstResult.ChiefComplaint.Should().Be(record1.ChiefComplaint);
        firstResult.ClinicalDetails.Should().ContainSingle();

        var secondResult = result.Items.First(r => r.Id == record2.Id);
        secondResult.ChiefComplaint.Should().Be(record2.ChiefComplaint);
        secondResult.ClinicalDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoRecordsExistForPatient()
    {
        // Arrange
        var request = new GetMedicalRecordsByPatientIdQuery(Guid.NewGuid(), 1, 10);

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(
                    request.PatientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<MedicalRecord>(), 0));

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    private static MedicalRecord CreateMedicalRecord(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    ) => MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);
}
