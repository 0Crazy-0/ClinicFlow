using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a clinical encounter record linked to a specific appointment.
/// Captures diagnosis, treatment, lab results, and follow-up instructions.
/// </summary>
public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; init; }

    public Guid DoctorId { get; init; }

    public Guid AppointmentId { get; init; }

    /// <summary>
    /// Indicates whether the minor's parent or guardian initiated this substance abuse treatment
    /// episode. Only meaningful when the protected care category is
    /// <see cref="ProtectedCategory.SubstanceAbuseTreatment"/>; null otherwise.
    /// </summary>
    /// <remarks>
    /// When true, the parent or guardian sought the care themselves, so Cal. Fam. Code § 6929(g)
    /// grants them an unconditional right to this record's information upon request, even if the
    /// minor does not consent to disclosure.
    /// When false, the minor consented to the treatment on their own, so no such unconditional
    /// disclosure right applies and the minor's consent governs access to the record.
    /// See <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6929">Cal. Fam. Code § 6929</see>.
    /// </remarks>
    public bool? GuardianInitiatedTreatment { get; init; }

    /// <summary>
    /// Reflects the treating professional's documented determination of whether involving the
    /// minor's parent or guardian was appropriate for this encounter. Only meaningful when the
    /// protected care category is <see cref="ProtectedCategory.MentalHealthCounseling"/> or
    /// <see cref="ProtectedCategory.ResidentialShelter"/>; null otherwise.
    /// </summary>
    /// <remarks>
    /// Under Cal. Fam. Code § 6924(d), guardian involvement is presumed unless the treating
    /// professional, after consulting with the minor, determines it would be inappropriate.
    /// When true, the documented determination supports guardian access to the record; when
    /// false, the professional determined that involvement would be inappropriate, and the
    /// record must state the reason in the treatment documentation.
    /// See <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&amp;sectionNum=6924">Cal. Fam. Code § 6924</see>.
    /// </remarks>
    public bool? GuardianInvolvementDeemedAppropriate { get; private set; }

    public ProtectedCategory? ProtectedCareCategory { get; init; }

    /// <summary>
    /// Primary symptom or reason for the visit as reported by the patient.
    /// </summary>
    public string ChiefComplaint { get; private set; } = string.Empty;

    private readonly List<DynamicClinicalDetail> _clinicalDetails = [];

    /// <summary>
    /// A structured collection of clinical details (e.g., Cardiology flags, Dental odontograms, etc.) collected during the encounter.
    /// </summary>
    public IReadOnlyCollection<DynamicClinicalDetail> ClinicalDetails =>
        _clinicalDetails.AsReadOnly();

    // EF Core constructor
    private MedicalRecord() { }

    private MedicalRecord(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint,
        ProtectedCategory? protectedCareCategory,
        bool? guardianInitiatedTreatment
    )
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentId = appointmentId;
        ProtectedCareCategory = protectedCareCategory;
        ChiefComplaint = chiefComplaint;
        GuardianInitiatedTreatment = guardianInitiatedTreatment;
    }

    internal static MedicalRecord Create(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint,
        ProtectedCategory? protectedCareCategory,
        bool? guardianInitiatedTreatment
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (doctorId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (appointmentId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (protectedCareCategory is not null && !Enum.IsDefined(protectedCareCategory.Value))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);

        if (
            protectedCareCategory is ProtectedCategory.SubstanceAbuseTreatment
            && guardianInitiatedTreatment is null
        )
            throw new DomainValidationException(
                DomainErrors.MedicalRecord.GuardianInitiatedTreatmentRequired
            );

        if (
            protectedCareCategory is not ProtectedCategory.SubstanceAbuseTreatment
            && guardianInitiatedTreatment is not null
        )
            throw new DomainValidationException(
                DomainErrors.MedicalRecord.GuardianInitiatedTreatmentNotApplicable
            );

        if (string.IsNullOrWhiteSpace(chiefComplaint))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        var record = new MedicalRecord(
            patientId,
            doctorId,
            appointmentId,
            chiefComplaint,
            protectedCareCategory,
            guardianInitiatedTreatment
        );

        return record;
    }

    internal void AddClinicalDetail(DynamicClinicalDetail detail)
    {
        if (detail is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (_clinicalDetails.Any(d => d.TemplateCode == detail.TemplateCode))
            throw new DomainValidationException(DomainErrors.MedicalEncounter.DetailAlreadyExists);

        _clinicalDetails.Add(detail);
    }

    internal void SetGuardianInvolvementDetermination(bool guardianInvolvementDeemedAppropriate)
    {
        if (
            ProtectedCareCategory
            is not (
                ProtectedCategory.MentalHealthCounseling
                or ProtectedCategory.ResidentialShelter
            )
        )
        {
            throw new DomainValidationException(
                DomainErrors.MedicalRecord.GuardianInvolvementFlagNotApplicable
            );
        }

        GuardianInvolvementDeemedAppropriate = guardianInvolvementDeemedAppropriate;
    }
}
