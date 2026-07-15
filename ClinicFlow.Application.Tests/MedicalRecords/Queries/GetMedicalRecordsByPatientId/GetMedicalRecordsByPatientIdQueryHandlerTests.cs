using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;
using ClinicFlow.Domain.Entities;
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
        var patientId = Guid.CreateVersion7();
        var request = new GetMedicalRecordsByPatientIdQuery(patientId, 1, 10);
        var record1 = CreateMedicalRecord(patientId);
        var record2 = CreateMedicalRecord(patientId);

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([record1, record2], 2));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<MedicalRecord> { record1, record2 }.Select(
            record => new MedicalRecordDto(
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
            )
        );

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoRecordsExistForPatient()
    {
        // Arrange
        var request = new GetMedicalRecordsByPatientIdQuery(Guid.CreateVersion7(), 1, 10);

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
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _medicalRecordRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedAsync(
                    request.PatientId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord(Guid patientId) =>
        MedicalRecord.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "chiefComplaint"
        );
}
