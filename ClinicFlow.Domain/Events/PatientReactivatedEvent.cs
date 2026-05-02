using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events;

public sealed record PatientReactivatedEvent(Guid PatientId) : IDomainEvent;
