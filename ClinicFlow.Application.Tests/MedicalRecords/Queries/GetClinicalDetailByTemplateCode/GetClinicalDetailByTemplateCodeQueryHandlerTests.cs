using ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
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
        var detail = new StubClinicalDetail("VITALS", """{"bp":"120/80"}""");
        record.AddClinicalDetail(detail);

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, CancellationToken.None))
            .ReturnsAsync(record);

        var query = new GetClinicalDetailByTemplateCodeQuery(record.Id, "VITALS");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TemplateCode.Should().Be("VITALS");
        result.JsonDataPayload.Should().Be("""{"bp":"120/80"}""");

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenDetailDoesNotExist()
    {
        // Arrange
        var record = CreateMedicalRecord();

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, CancellationToken.None))
            .ReturnsAsync(record);

        var query = new GetClinicalDetailByTemplateCodeQuery(record.Id, "NON_EXISTENT");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, CancellationToken.None),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMedicalRecordNotFound()
    {
        // Arrange
        var medicalRecordId = Guid.NewGuid();

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, CancellationToken.None))
            .ReturnsAsync((MedicalRecord?)null);

        var query = new GetClinicalDetailByTemplateCodeQuery(medicalRecordId, "VITALS");

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(medicalRecordId, CancellationToken.None),
            Times.Once
        );
    }

    private static MedicalRecord CreateMedicalRecord() =>
        MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "General checkup");

    private class StubClinicalDetail(string templateCode, string jsonDataPayload)
        : IClinicalDetailRecord
    {
        public string TemplateCode => templateCode;
        public string JsonDataPayload => jsonDataPayload;
    }
}
