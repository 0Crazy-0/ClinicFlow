using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Infrastructure.Persistence.Seeding.Data;

public sealed record AppointmentTypeSeedItem
{
    public required AppointmentCategory Category { get; init; }
    public required string Name { get; init; }
    public required string Desc { get; init; }
    public required int Duration { get; init; }
    public required string TemplateCode { get; init; }
    public required string SpecialtyName { get; init; }
    public required AgeEligibilityPolicy? AgePolicy { get; init; }
}

/// <summary>
/// Provides predefined appointment type seed data.
/// </summary>
public static class AppointmentTypeDefinitionData
{
    public static readonly IReadOnlySet<string> TypesToDeactivate = new HashSet<string>
    {
        "Chronic Digestive Disease Review",
        "Mental Health Crisis Evaluation",
    };

    public static IReadOnlyList<AppointmentTypeSeedItem> GetSeedItems() =>
        [
            // General Medicine
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "General Adult Consultation",
                Desc = "Initial assessment and comprehensive checkup for adult patients.",
                Duration = 20,
                TemplateCode = "GEN_INTAKE_V1",
                SpecialtyName = "General Medicine",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Chronic Disease Follow-up",
                Desc =
                    "Routine review of chronic conditions like hypertension, diabetes, or asthma.",
                Duration = 25,
                TemplateCode = "GEN_INTAKE_V1",
                SpecialtyName = "General Medicine",
                AgePolicy = null,
            },
            // Pediatrics
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Checkup,
                Name = "Pediatric Well-Child Checkup",
                Desc = "Routine developmental checkup and vaccination review for children.",
                Duration = 20,
                TemplateCode = "PED_INTAKE_V1",
                SpecialtyName = "Pediatrics",
                AgePolicy = AgeEligibilityPolicy.Create(null, 17, true),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Pediatric Initial Consultation",
                Desc = "Initial diagnosis and treatment planning for pediatric patients.",
                Duration = 20,
                TemplateCode = "PED_INTAKE_V1",
                SpecialtyName = "Pediatrics",
                AgePolicy = AgeEligibilityPolicy.Create(null, 17, true),
            },
            // Cardiology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Cardiology Consultation",
                Desc = "Initial cardiovascular assessment and treatment planning.",
                Duration = 45,
                TemplateCode = "CARD_VITALS_V1",
                SpecialtyName = "Cardiology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Checkup,
                Name = "Cardiovascular Risk Assessment",
                Desc = "Comprehensive review of cardiac risk factors and extended vitals.",
                Duration = 40,
                TemplateCode = "CARD_VITALS_V2",
                SpecialtyName = "Cardiology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Procedure,
                Name = "Electrocardiogram (ECG)",
                Desc = "Diagnostic ECG recording and interpretation.",
                Duration = 20,
                TemplateCode = "CARD_ECG_V1",
                SpecialtyName = "Cardiology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            // Dermatology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Skin Lesion Evaluation",
                Desc = "Detailed examination and dermoscopy of suspicious skin lesions.",
                Duration = 15,
                TemplateCode = "DERM_LESION_V1",
                SpecialtyName = "Dermatology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Acne Treatment Follow-up",
                Desc = "Progress evaluation for ongoing dermatological therapy.",
                Duration = 15,
                TemplateCode = "DERM_LESION_V1",
                SpecialtyName = "Dermatology",
                AgePolicy = null,
            },
            // Gynaecology
            ///<remarks>
            /// Legal guardian not required: adolescent reproductive health
            /// consultations are confidential by clinical policy.
            ///</remarks>
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Gynecological Assessment",
                Desc = "Comprehensive gynecological screening and history taking.",
                Duration = 30,
                TemplateCode = "GYN_CONSULT_V1",
                SpecialtyName = "Gynaecology",
                AgePolicy = AgeEligibilityPolicy.Create(12, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Prenatal Care Consultation",
                Desc = "Routine maternal-fetal health tracking and obstetric checkup.",
                Duration = 30,
                TemplateCode = "GYN_CONSULT_V1",
                SpecialtyName = "Gynaecology",
                AgePolicy = AgeEligibilityPolicy.Create(12, null, false),
            },
            // Ophthalmology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Checkup,
                Name = "Comprehensive Visual Acuity Exam",
                Desc = "Standard visual acuity, slit lamp, and visual fields assessment.",
                Duration = 20,
                TemplateCode = "OPH_EXAM_V1",
                SpecialtyName = "Ophthalmology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Intraocular Pressure Monitoring",
                Desc = "Glaucoma follow-up and tonometry check.",
                Duration = 15,
                TemplateCode = "OPH_EXAM_V1",
                SpecialtyName = "Ophthalmology",
                AgePolicy = null,
            },
            // Orthopedics
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Musculoskeletal Injury Evaluation",
                Desc = "Comprehensive orthopedic evaluation of joints, bones, and muscles.",
                Duration = 40,
                TemplateCode = "ORT_MUSCULO_V1",
                SpecialtyName = "Orthopedics",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Post-Fracture Recovery Follow-up",
                Desc = "Reviewing mobility progress and recovery status of skeletal injuries.",
                Duration = 20,
                TemplateCode = "ORT_MUSCULO_V1",
                SpecialtyName = "Orthopedics",
                AgePolicy = null,
            },
            // Otolaryngology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "ENT Initial Consultation",
                Desc = "Specialist review of ear, nose, throat, or balance symptoms.",
                Duration = 20,
                TemplateCode = "OTO_EXAM_V1",
                SpecialtyName = "Otolaryngology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Checkup,
                Name = "Hearing Loss Assessment",
                Desc = "Detailed audiometry interpretation and otoscopic exam.",
                Duration = 30,
                TemplateCode = "OTO_EXAM_V1",
                SpecialtyName = "Otolaryngology",
                AgePolicy = null,
            },
            // Neurology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Neurological Evaluation",
                Desc = "Initial specialist motor, reflex, and cognitive assessment.",
                Duration = 45,
                TemplateCode = "NEUR_REFLEX_V1",
                SpecialtyName = "Neurology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Brain MRI & Neuro-imaging Review",
                Desc = "Specialist analysis of neurology imaging results and plan adjustment.",
                Duration = 30,
                TemplateCode = "NEUR_IMAGING_V1",
                SpecialtyName = "Neurology",
                AgePolicy = null,
            },
            // Psychiatry
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Psychiatric Initial Intake",
                Desc = "Comprehensive mental status exam and initial psychiatric assessment.",
                Duration = 60,
                TemplateCode = "PSYC_SESSION_V1",
                SpecialtyName = "Psychiatry",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Emergency,
                Name = "Mental Health Crisis Evaluation",
                Desc = "Immediate psychiatric risk assessment and safety plan design.",
                Duration = 45,
                TemplateCode = "PSYC_RISK_V1",
                SpecialtyName = "Psychiatry",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Therapeutic Medication Follow-up",
                Desc = "Mood assessment and medication tuning for existing psychiatric patients.",
                Duration = 45,
                TemplateCode = "PSYC_SESSION_V1",
                SpecialtyName = "Psychiatry",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            // Urology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Urological Initial Consultation",
                Desc = "Urinary and reproductive system diagnostic evaluation.",
                Duration = 30,
                TemplateCode = "URO_CONSULT_V1",
                SpecialtyName = "Urology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.Checkup,
                Name = "Prostate Cancer Prevention Screening",
                Desc = "Annual checkup including prostate exam and PSA review.",
                Duration = 20,
                TemplateCode = "URO_CONSULT_V1",
                SpecialtyName = "Urology",
                AgePolicy = AgeEligibilityPolicy.Create(40, null, false),
            },
            // Oncology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Oncology Initial Consultation",
                Desc = "Comprehensive cancer diagnosis review, staging and treatment planning.",
                Duration = 60,
                TemplateCode = "ONCO_FOLLOWUP_V1",
                SpecialtyName = "Oncology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Chemotherapy Tolerance Checkup",
                Desc = "Reviewing toxicities and blood counts before next chemotherapy cycle.",
                Duration = 45,
                TemplateCode = "ONCO_FOLLOWUP_V1",
                SpecialtyName = "Oncology",
                AgePolicy = AgeEligibilityPolicy.Create(18, null, false),
            },
            // Endocrinology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Endocrine Disorders Intake",
                Desc = "Comprehensive hormonal and thyroid disease diagnostic check.",
                Duration = 40,
                TemplateCode = "ENDO_HORMONAL_V1",
                SpecialtyName = "Endocrinology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Diabetes Mellitus Follow-up",
                Desc = "Metabolic panel analysis, HbA1c review and insulin adjustment.",
                Duration = 30,
                TemplateCode = "ENDO_HORMONAL_V1",
                SpecialtyName = "Endocrinology",
                AgePolicy = null,
            },
            // Gastroenterology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Gastroenterology Intake",
                Desc = "Specialist diagnostic evaluation of persistent digestive disorders.",
                Duration = 35,
                TemplateCode = "GAST_DIGEST_V1",
                SpecialtyName = "Gastroenterology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Chronic Digestive Disease Review",
                Desc = "Monitoring progress of conditions like Crohn's, IBS or gastritis.",
                Duration = 20,
                TemplateCode = "GAST_DIGEST_V1",
                SpecialtyName = "Gastroenterology",
                AgePolicy = null,
            },
            // Pulmonology
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FirstConsultation,
                Name = "Pulmonology Comprehensive Exam",
                Desc = "Comprehensive lung function, spirometry review and respiratory intake.",
                Duration = 30,
                TemplateCode = "PULM_RESPIRATORY_V1",
                SpecialtyName = "Pulmonology",
                AgePolicy = null,
            },
            new AppointmentTypeSeedItem
            {
                Category = AppointmentCategory.FollowUp,
                Name = "Asthma & COPD Control Check",
                Desc = "Assessing oxygen saturation, smoking history and lung function.",
                Duration = 25,
                TemplateCode = "PULM_RESPIRATORY_V1",
                SpecialtyName = "Pulmonology",
                AgePolicy = null,
            },
        ];
}
