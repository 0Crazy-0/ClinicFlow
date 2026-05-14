using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Patients;

public sealed record PatientReactivatedEvent(Guid PatientId) : IDomainEvent;
