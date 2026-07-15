using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public class GetClinicalDetailByTemplateCodeQueryHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly GetClinicalDetailByTemplateCodeQueryHandler _sut;

    public GetClinicalDetailByTemplateCodeQueryHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _sut = new GetClinicalDetailByTemplateCodeQueryHandler(_medicalRecordRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnClinicalDetailDto_WhenDetailExists()
    {
        // Arrange
        var record = CreateMedicalRecord();
        var detail = DynamicClinicalDetail.Create("VITALS", """{"bp":"120/80"}""");
        record.AddClinicalDetail(detail);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(record);

        var query = new GetClinicalDetailByTemplateCodeQuery(record.Id, "VITALS");

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new ClinicalDetailDto(detail.TemplateCode, detail.JsonDataPayload);

        result.Should().BeEquivalentTo(expectedDto);

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, TestContext.Current.CancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenDetailDoesNotExist()
    {
        // Arrange
        var record = CreateMedicalRecord();

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, TestContext.Current.CancellationToken))
            .ReturnsAsync(record);

        var query = new GetClinicalDetailByTemplateCodeQuery(record.Id, "NON_EXISTENT");

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, TestContext.Current.CancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMedicalRecordNotFound()
    {
        // Arrange
        var medicalRecordId = Guid.CreateVersion7();

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, TestContext.Current.CancellationToken))
            .ReturnsAsync((MedicalRecord?)null);

        var query = new GetClinicalDetailByTemplateCodeQuery(medicalRecordId, "VITALS");

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(medicalRecordId, TestContext.Current.CancellationToken),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "General checkup"
        );
}
