using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public class GetMedicalRecordsByDoctorIdQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetMedicalRecordsByDoctorIdQueryHandler _sut;

    public GetMedicalRecordsByDoctorIdQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetMedicalRecordsByDoctorIdQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenRecordsExistForDoctor()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var request = new GetMedicalRecordsByDoctorIdQuery(doctorId, 1, 10);

        var record1 = CreateMedicalRecord(doctorId);
        var record2 = CreateMedicalRecord(doctorId);

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdPaginatedAsync(doctorId, 1, 10, It.IsAny<CancellationToken>())
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
            x => x.GetByDoctorIdPaginatedAsync(doctorId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoRecordsExistForDoctor()
    {
        // Arrange
        var request = new GetMedicalRecordsByDoctorIdQuery(Guid.CreateVersion7(), 1, 10);

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdPaginatedAsync(
                    request.DoctorId,
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
                x.GetByDoctorIdPaginatedAsync(
                    request.DoctorId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord(Guid doctorId) =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            "chiefComplaint"
        );
}
