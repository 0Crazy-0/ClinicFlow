using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public class AddClinicalDetailToMedicalRecordCommandHandler(IMedicalRecordRepository medicalRecordRepository, IClinicalFormTemplateRepository templateRepository,
    MedicalEncounterService medicalEncounterService, IUnitOfWork unitOfWork) : IRequestHandler<AddClinicalDetailToMedicalRecordCommand>
{
    public async Task Handle(AddClinicalDetailToMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(MedicalRecord), request.MedicalRecordId);

        var template = await templateRepository.GetByCodeAsync(request.Detail.TemplateCode, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrors.General.NotFound, nameof(ClinicalFormTemplate), request.Detail.TemplateCode);

        var detail = DynamicClinicalDetail.Create(request.Detail.TemplateCode, request.Detail.JsonDataPayload);

        medicalEncounterService.AppendClinicalDetail(record, detail, template);

        await medicalRecordRepository.UpdateAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
