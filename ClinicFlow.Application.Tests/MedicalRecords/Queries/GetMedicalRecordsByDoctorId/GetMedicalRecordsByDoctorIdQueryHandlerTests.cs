using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
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
        var doctorId = Guid.NewGuid();
        var request = new GetMedicalRecordsByDoctorIdQuery(doctorId, 1, 10);

        var record1 = CreateMedicalRecord(Guid.NewGuid(), doctorId, Guid.NewGuid(), "Headache");
        var record2 = CreateMedicalRecord(Guid.NewGuid(), doctorId, Guid.NewGuid(), "Fever");

        record1.AddClinicalDetail(DynamicClinicalDetail.Create("vital-signs", "{}"));

        _medicalRecordRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdPaginatedAsync(doctorId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(([record1, record2], 2));

        // Act
        var result = await _sut.Handle(request, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().HaveCount(2);

        var firstResult = result.Items.First(r => r.Id == record1.Id);
        firstResult.DoctorId.Should().Be(doctorId);
        firstResult.ChiefComplaint.Should().Be("Headache");
        firstResult.ClinicalDetails.Should().ContainSingle();

        var secondResult = result.Items.First(r => r.Id == record2.Id);
        secondResult.DoctorId.Should().Be(doctorId);
        secondResult.ChiefComplaint.Should().Be("Fever");
        secondResult.ClinicalDetails.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoRecordsExistForDoctor()
    {
        // Arrange
        var request = new GetMedicalRecordsByDoctorIdQuery(Guid.NewGuid(), 1, 10);

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
