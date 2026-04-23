using ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Policies;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public class AddClinicalDetailToMedicalRecordCommandHandlerTests
{
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IClinicalFormTemplateRepository> _templateRepositoryMock;
    private readonly Mock<IJsonSchemaValidator> _jsonValidatorMock;
    private readonly MedicalEncounterService _medicalEncounterService;
    private readonly AddClinicalDetailToMedicalRecordCommandHandler _sut;

    public AddClinicalDetailToMedicalRecordCommandHandlerTests()
    {
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _templateRepositoryMock = new Mock<IClinicalFormTemplateRepository>();
        _jsonValidatorMock = new Mock<IJsonSchemaValidator>();

        var policies = new List<IMedicalRecordValidationPolicy>();
        _medicalEncounterService = new MedicalEncounterService(policies, _jsonValidatorMock.Object);

        _sut = new AddClinicalDetailToMedicalRecordCommandHandler(
            _medicalRecordRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _medicalEncounterService,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_GivenValidRequest_AddsDetailAndSaves()
    {
        // Arrange
        var medicalRecordId = Guid.NewGuid();
        var request = new AddClinicalDetailToMedicalRecordCommand(
            medicalRecordId,
            "lab-results",
            """{"glucose": 90}"""
        );

        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Checkup");
        var template = CreateFormTemplate("lab-results");

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, CancellationToken.None))
            .ReturnsAsync(record);

        _templateRepositoryMock
            .Setup(x => x.GetByCodeAsync("lab-results", CancellationToken.None))
            .ReturnsAsync(template);

        string? errorMessage = null;
        _jsonValidatorMock
            .Setup(x => x.ValidateSchema(It.IsAny<string>(), It.IsAny<string>(), out errorMessage))
            .Returns(true);

        // Act
        await _sut.Handle(request, CancellationToken.None);

        // Assert
        record
            .ClinicalDetails.Should()
            .ContainSingle(d =>
                d.TemplateCode == "lab-results" && d.JsonDataPayload == """{"glucose": 90}"""
            );

        _medicalRecordRepositoryMock.Verify(
            x => x.UpdateAsync(record, CancellationToken.None),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidMedicalRecordId_ThrowsEntityNotFoundException()
    {
        // Arrange
        var medicalRecordId = Guid.NewGuid();
        var request = new AddClinicalDetailToMedicalRecordCommand(
            medicalRecordId,
            "lab-results",
            "{}"
        );

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, CancellationToken.None))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));
        _templateRepositoryMock.Verify(
            x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _medicalRecordRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GivenInvalidTemplateCode_ThrowsEntityNotFoundException()
    {
        // Arrange
        var medicalRecordId = Guid.NewGuid();
        var request = new AddClinicalDetailToMedicalRecordCommand(
            medicalRecordId,
            "invalid-code",
            "{}"
        );
        var record = CreateMedicalRecord(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Checkup");

        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(medicalRecordId, CancellationToken.None))
            .ReturnsAsync(record);

        _templateRepositoryMock
            .Setup(x => x.GetByCodeAsync("invalid-code", CancellationToken.None))
            .ReturnsAsync((ClinicalFormTemplate?)null);

        // Act
        var act = async () => await _sut.Handle(request, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(ClinicalFormTemplate));

        _medicalRecordRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static MedicalRecord CreateMedicalRecord(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    ) => MedicalRecord.Create(patientId, doctorId, appointmentId, chiefComplaint);

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    ) => ClinicalFormTemplate.Create(code, "Test Form", "Description", jsonSchema);
}
