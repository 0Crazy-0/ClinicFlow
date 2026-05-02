using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public sealed class AddClinicalDetailToMedicalRecordCommandHandler(
    IMedicalRecordRepository medicalRecordRepository,
    IClinicalFormTemplateRepository templateRepository,
    MedicalEncounterService medicalEncounterService,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddClinicalDetailToMedicalRecordCommand>
{
    public async Task Handle(
        AddClinicalDetailToMedicalRecordCommand request,
        CancellationToken cancellationToken
    )
    {
        var record =
            await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecord),
                request.MedicalRecordId
            );

        var template =
            await templateRepository.GetByCodeAsync(request.TemplateCode, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateCode
            );

        var detail = DynamicClinicalDetail.Create(request.TemplateCode, request.JsonDataPayload);

        medicalEncounterService.AppendClinicalDetail(record, detail, template);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
