using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events.MedicalRecords;

public sealed record MedicalRecordCreatedEvent(MedicalRecord MedicalRecord) : IDomainEvent;
