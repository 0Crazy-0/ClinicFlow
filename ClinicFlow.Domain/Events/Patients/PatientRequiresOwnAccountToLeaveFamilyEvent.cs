using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Patients;

public sealed record PatientRequiresOwnAccountToLeaveFamilyEvent(Guid PatientId) : IDomainEvent;
