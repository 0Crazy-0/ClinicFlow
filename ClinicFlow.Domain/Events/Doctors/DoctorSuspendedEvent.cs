using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Doctors;

public sealed record DoctorSuspendedEvent(Guid DoctorId) : IDomainEvent;
