using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when a new medical record is created for a patient encounter.
/// </summary>
public record MedicalRecordCreatedEvent(MedicalRecord MedicalRecord) : IDomainEvent;