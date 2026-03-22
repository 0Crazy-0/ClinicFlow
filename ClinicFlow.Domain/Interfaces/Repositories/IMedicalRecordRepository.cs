using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalRecord"/> persistence operations.
/// </summary>
public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IList<MedicalRecord>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    Task<IList<MedicalRecord>> GetByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default
    );

    Task<MedicalRecord?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default
    );

    Task<MedicalRecord> CreateAsync(
        MedicalRecord medicalRecord,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
