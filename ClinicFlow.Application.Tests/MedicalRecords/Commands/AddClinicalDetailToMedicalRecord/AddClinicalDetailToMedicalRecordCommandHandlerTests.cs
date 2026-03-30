using System.Reflection;
using ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;
using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
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
            new DynamicClinicalDetailDto("lab-results", "{\"glucose\": 90}")
        );

        var record = CreateMedicalRecord(
            medicalRecordId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Checkup"
        );
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
                d.TemplateCode == "lab-results" && d.JsonDataPayload == "{\"glucose\": 90}"
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
            new DynamicClinicalDetailDto("lab-results", "{}")
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
            new DynamicClinicalDetailDto("invalid-code", "{}")
        );
        var record = CreateMedicalRecord(
            medicalRecordId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Checkup"
        );

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
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    )
    {
        var record = (MedicalRecord)Activator.CreateInstance(typeof(MedicalRecord), true)!;
        SetPrivateProperty(record, nameof(MedicalRecord.Id), id);
        SetPrivateProperty(record, nameof(MedicalRecord.PatientId), patientId);
        SetPrivateProperty(record, nameof(MedicalRecord.DoctorId), doctorId);
        SetPrivateProperty(record, nameof(MedicalRecord.AppointmentId), appointmentId);
        SetPrivateProperty(record, nameof(MedicalRecord.ChiefComplaint), chiefComplaint);
        return record;
    }

    private static ClinicalFormTemplate CreateFormTemplate(
        string code = "Test1",
        string jsonSchema = "{}"
    )
    {
        var template = (ClinicalFormTemplate)
            Activator.CreateInstance(typeof(ClinicalFormTemplate), true)!;
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.Id), Guid.NewGuid());
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.Code), code);
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.Name), "Test Form");
        SetPrivateProperty(template, nameof(ClinicalFormTemplate.JsonSchemaDefinition), jsonSchema);
        return template;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }
}
