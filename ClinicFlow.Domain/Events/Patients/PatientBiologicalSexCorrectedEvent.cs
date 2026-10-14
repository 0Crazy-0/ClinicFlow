using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Events.Patients;

public sealed record PatientBiologicalSexCorrectedEvent(
    Guid PatientId,
    BiologicalSex PreviousSex,
    BiologicalSex CorrectedSex,
    Guid CorrectedByDoctorId,
    string Reason,
    DateTime CorrectedAt
) : IDomainEvent;
