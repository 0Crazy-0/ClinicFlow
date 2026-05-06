using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events;

public sealed record DoctorSuspendedEvent(Guid DoctorId) : IDomainEvent;
