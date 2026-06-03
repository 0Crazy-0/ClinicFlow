using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Infrastructure.Persistence.Seeding;

/// <summary>
/// Generates deterministic, valid appointments for database seeding using pre-calculated lists of doctors, patients, schedules, and users.
/// </summary>
public class AppointmentGenerator(AppointmentSeedingArgs args, DateTime baseDate)
{
    private readonly IReadOnlyList<Doctor> _activeDoctors =
    [
        .. args.Doctors.Where(d => !d.IsDeleted),
    ];

    private readonly IReadOnlyList<Patient> _activePatients =
    [
        .. args.Patients.Where(p => !p.IsDeleted),
    ];

    private readonly IReadOnlyDictionary<Guid, User> _patientUsersById =
        args.PatientUsers.ToDictionary(u => u.Id);

    private static readonly IReadOnlyList<AppointmentStatus> TargetStatuses =
    [
        .. Enumerable.Repeat(AppointmentStatus.Completed, 250),
        .. Enumerable.Repeat(AppointmentStatus.Scheduled, 100),
        .. Enumerable.Repeat(AppointmentStatus.Cancelled, 50),
        .. Enumerable.Repeat(AppointmentStatus.LateCancellation, 40),
        .. Enumerable.Repeat(AppointmentStatus.NoShow, 35),
        .. Enumerable.Repeat(AppointmentStatus.CheckedIn, 15),
        .. Enumerable.Repeat(AppointmentStatus.InProgress, 10),
    ];

    public Appointment Generate(int index)
    {
        var status = TargetStatuses[index];
        var apptType = args.AppointmentTypes[index % args.AppointmentTypes.Count];
        var doctor = GetEligibleDoctor(apptType, index);
        var patient = GetEligiblePatient(apptType, index);
        var schedule = GetDoctorSchedule(doctor.Id, index);
        var apptDate = ResolveAppointmentDate(schedule, status, index, baseDate);
        var timeRange = GetTimeRangeForAppointment(schedule, apptType, index);
        var patientNotes = GetPatientNotes(apptType.Name, index);

        Appointment appointment;

        if (index % 8 is 3)
        {
            var originalDate = apptDate.AddDays(-7);
            appointment = Appointment.Schedule(
                patient.Id,
                doctor.Id,
                apptType.Id,
                originalDate,
                timeRange,
                patientNotes
            );

            appointment.Reschedule(apptDate, timeRange);
        }
        else
        {
            appointment = Appointment.Schedule(
                patient.Id,
                doctor.Id,
                apptType.Id,
                apptDate,
                timeRange,
                patientNotes
            );
        }

        var actionTime = apptDate.Add(timeRange.Start).AddMinutes(-10);
        string? receptionistNotes = status
            is AppointmentStatus.Completed
                or AppointmentStatus.CheckedIn
                or AppointmentStatus.InProgress
            ? GetReceptionistNotes(status, index)
            : null;

        TransitionAppointmentToStatus(
            appointment,
            status,
            doctor.Id,
            _patientUsersById[patient.UserId].Id,
            actionTime,
            receptionistNotes,
            index
        );

        return appointment;
    }

    private Doctor GetEligibleDoctor(AppointmentTypeDefinition apptType, int index)
    {
        if (apptType.IsUnrestrictedBySpecialty)
            return _activeDoctors[index % _activeDoctors.Count];

        var eligible = _activeDoctors
            .Where(d => apptType.AllowedSpecialtyIds.Contains(d.MedicalSpecialtyId))
            .ToList();

        return eligible.Count > 0
            ? eligible[index % eligible.Count]
            : _activeDoctors[index % _activeDoctors.Count];
    }

    private Patient GetEligiblePatient(AppointmentTypeDefinition apptType, int index)
    {
        var hasAgeRestrictions =
            apptType.AgePolicy.MinimumAge.HasValue || apptType.AgePolicy.MaximumAge.HasValue;

        if (!hasAgeRestrictions)
            return _activePatients[index % _activePatients.Count];

        var eligible = _activePatients
            .Where(p => IsAgeEligible(p.DateOfBirth, apptType.AgePolicy))
            .ToList();

        return eligible.Count > 0
            ? eligible[index % eligible.Count]
            : _activePatients[index % _activePatients.Count];
    }

    private bool IsAgeEligible(DateTime dateOfBirth, AgeEligibilityPolicy agePolicy)
    {
        int age = baseDate.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > baseDate.AddYears(-age))
            age--;

        bool minMet = !agePolicy.MinimumAge.HasValue || age >= agePolicy.MinimumAge.Value;
        bool maxMet = !agePolicy.MaximumAge.HasValue || age <= agePolicy.MaximumAge.Value;

        return minMet && maxMet;
    }

    private Schedule GetDoctorSchedule(Guid doctorId, int index)
    {
        var docSchedules = args.Schedules.Where(s => s.DoctorId == doctorId).ToList();
        return docSchedules.Count > 0
            ? docSchedules[index % docSchedules.Count]
            : args.Schedules[index % args.Schedules.Count];
    }

    private static TimeRange GetTimeRangeForAppointment(
        Schedule schedule,
        AppointmentTypeDefinition apptType,
        int index
    )
    {
        var scheduleStart = schedule.TimeRange.Start.Hours;
        var scheduleEnd = schedule.TimeRange.End.Hours;
        var durationHours = (int)Math.Ceiling(apptType.DurationMinutes.TotalHours);
        var maxStart = Math.Max(scheduleStart, scheduleEnd - durationHours);
        var startHour = Math.Min(scheduleStart + (index % 4), maxStart);

        return TimeRange.Create(
            TimeSpan.FromHours(startHour),
            TimeSpan.FromHours(startHour) + apptType.DurationMinutes
        );
    }

    private static DateTime GetNextWeekday(DateTime baseDate, DayOfWeek targetDay)
    {
        int daysUntilTarget = ((int)targetDay - (int)baseDate.DayOfWeek + 7) % 7;

        return baseDate.AddDays(daysUntilTarget is 0 ? 7 : daysUntilTarget);
    }

    private static DateTime ResolveAppointmentDate(
        Schedule schedule,
        AppointmentStatus status,
        int index,
        DateTime baseDate
    )
    {
        var date = GetNextWeekday(baseDate, schedule.DayOfWeek);

        bool isPast =
            status
            is AppointmentStatus.Completed
                or AppointmentStatus.Cancelled
                or AppointmentStatus.LateCancellation
                or AppointmentStatus.NoShow;

        if (isPast)
        {
            var weeksBack = (index * 17 % 26) + 1;
            return date.AddDays(-7 * weeksBack);
        }

        var weeksAhead = (index * 3 % 4) + 1;

        return date.AddDays(7 * weeksAhead);
    }

    private static void TransitionAppointmentToStatus(
        Appointment appointment,
        AppointmentStatus status,
        Guid doctorId,
        Guid patientUserId,
        DateTime actionTime,
        string? receptionistNotes,
        int index
    )
    {
        switch (status)
        {
            case AppointmentStatus.Completed:
                appointment.CheckIn(actionTime, receptionistNotes);
                appointment.Start(doctorId, actionTime.AddMinutes(5));
                appointment.Complete(actionTime.AddMinutes(35));
                break;

            case AppointmentStatus.Cancelled:
                if (index % 5 is 0) // 20% of cancelled appointments are system timeouts
                {
                    appointment.MarkAsRequiresReassignment();
                    appointment.CancelDueToSystemTimeout(actionTime.AddDays(-1));
                }
                else
                {
                    var reason = GetCancellationReason(index);
                    appointment.Cancel(patientUserId, reason, actionTime.AddDays(-2));
                }
                break;

            case AppointmentStatus.LateCancellation:
                var lateReason = GetLateCancellationReason(index);
                appointment.CancelLate(patientUserId, lateReason, actionTime.AddMinutes(-30));
                break;

            case AppointmentStatus.NoShow:
                appointment.MarkAsNoShowByStaff();
                break;

            case AppointmentStatus.CheckedIn:
                appointment.CheckIn(actionTime, receptionistNotes);
                break;

            case AppointmentStatus.InProgress:
                appointment.CheckIn(actionTime, receptionistNotes);
                appointment.Start(doctorId, actionTime.AddMinutes(5));
                break;
        }
    }

    private static string GetPatientNotes(string apptTypeName, int index)
    {
        string[] notes = apptTypeName switch
        {
            "General Adult Consultation" =>
            [
                "Requires a general physical exam and a checkup on recent blood test results.",
                "Has been feeling fatigued lately and wants a general medical evaluation.",
                "Experiencing occasional tension headaches and mild sleep issues.",
                "Needs a medical certificate and a general checkup.",
                "Experiencing mild digestive discomfort and bloating after meals.",
            ],
            "Chronic Disease Follow-up" =>
            [
                "Routine checkup for hypertension; blood pressure has been stable around 130/80.",
                "Follow-up for type 2 diabetes. Need to review recent HbA1c results and adjust metformin.",
                "Review of asthma symptoms and renewal of inhaler prescription.",
                "Routine check for hypercholesterolemia. Need to review lipid panel.",
                "Follow-up on chronic lower back pain management plan.",
            ],
            "Pediatric Well-Child Checkup" =>
            [
                "Routine 2-year growth and developmental milestone checkup.",
                "Scheduled vaccination review and routine pediatric checkup.",
                "Growth and nutrition assessment for a toddler.",
                "Routine checkup for infant development and vaccination schedule.",
                "School entry physical checkup and vaccination verification.",
            ],
            "Pediatric Initial Consultation" =>
            [
                "Persistent dry cough for a week, no fever but poor sleep.",
                "Recurring stomach aches in the morning before school.",
                "Evaluation for mild skin rash on arms and torso.",
                "Evaluation for suspected food allergy after hives occurred.",
                "Consultation regarding persistent bedwetting issues.",
            ],
            "Cardiology Consultation" =>
            [
                "Experiencing occasional chest tightness during mild exercise.",
                "Evaluation of heartbeat palpitations and mild shortness of breath.",
                "Referred by general practitioner due to high blood pressure readings.",
                "Family history of early coronary artery disease; seeking screening.",
                "Experiencing lightheadedness and feeling faint when standing up quickly.",
            ],
            "Cardiovascular Risk Assessment" =>
            [
                "Routine assessment of cardiac risk due to high cholesterol and smoking history.",
                "Comprehensive review of cardiovascular health and fitness levels.",
                "Evaluating overall heart health before starting a high-intensity exercise program.",
                "Assessment of high-risk profile due to obesity, diabetes, and family history.",
                "Annual cardiovascular risk checkup and lifestyle recommendation review.",
            ],
            "Electrocardiogram (ECG)" =>
            [
                "Diagnostic ECG requested by cardiologist to evaluate irregular heart rhythm.",
                "Pre-operative ECG screening before scheduled outpatient surgery.",
                "ECG to investigate recent episodes of unexplained tachycardia.",
                "Follow-up ECG to monitor heart activity after medication change.",
                "ECG requested during routine physical due to borderline heart sounds.",
            ],
            "Skin Lesion Evaluation" =>
            [
                "Wants a mole on the upper back checked; it seems to have changed color and size.",
                "Evaluating a persistent scaly patch of skin on the forehead that won't heal.",
                "Checkup for multiple new spots on shoulders after a sunburn.",
                "Evaluation of a fast-growing, dark lesion on the right thigh.",
                "Full-body screening for moles due to family history of melanoma.",
            ],
            "Acne Treatment Follow-up" =>
            [
                "Review of progress with topical retinoids and oral antibiotics. Skin is clearing up.",
                "Follow-up on isotretinoin (Accutane) therapy; need to review blood work.",
                "Evaluation of skin dryness and irritation from current acne medication.",
                "Review of hormonal acne treatment progress. Seeking alternative options.",
                "Routine follow-up to assess scarring reduction treatment.",
            ],
            "Gynecological Assessment" =>
            [
                "Annual routine screening, pelvic exam, and Pap smear.",
                "Experiencing irregular menstrual cycles and moderate pelvic pain.",
                "Seeking consultation for severe premenstrual syndrome (PMS) symptoms.",
                "Discussion regarding contraceptive options and family planning.",
                "Evaluation of persistent vaginal irritation and unusual discharge.",
            ],
            "Prenatal Care Consultation" =>
            [
                "Routine prenatal checkup at 20 weeks; need to review anatomy ultrasound.",
                "First prenatal visit. Estimated gestational age is 8 weeks.",
                "Routine third-trimester prenatal checkup; monitoring blood pressure.",
                "Prenatal follow-up; discussing birth plan and glucose tolerance test results.",
                "Routine 32-week checkup; monitoring fetal movements and heart rate.",
            ],
            "Comprehensive Visual Acuity Exam" =>
            [
                "Experiencing progressive difficulty reading small print and night driving.",
                "Routine eye exam; needs updated prescription for glasses.",
                "Frequent eye strain and dry eyes after long hours working on the computer.",
                "Blurred vision in the left eye; wants a comprehensive checkup.",
                "Seeking evaluation for recurring headaches related to eye strain.",
            ],
            "Intraocular Pressure Monitoring" =>
            [
                "Routine glaucoma checkup to measure intraocular pressure in both eyes.",
                "Follow-up to monitor pressure levels after starting new eye drops.",
                "Regular screening due to borderline high intraocular pressure in previous exam.",
                "Monitoring ocular hypertension to prevent optic nerve damage.",
                "Post-treatment checkup for glaucoma management.",
            ],
            "Musculoskeletal Injury Evaluation" =>
            [
                "Severe knee pain and swelling after twisting it during a soccer match.",
                "Persistent right shoulder pain when lifting objects above shoulder level.",
                "Evaluation of acute lower back pain after lifting heavy boxes.",
                "Sprained left ankle; swelling has not gone down after 3 days of rest.",
                "Chronic wrist pain, possibly carpal tunnel syndrome, worsening at night.",
            ],
            "Post-Fracture Recovery Follow-up" =>
            [
                "Follow-up X-ray and mobility check 6 weeks after wrist fracture.",
                "Routine evaluation after cast removal; starting physical therapy.",
                "Review of bone healing progress for a fractured ankle.",
                "Evaluating range of motion and pain levels after collarbone fracture.",
                "Final checkup post-fracture; assessing bone density and strength.",
            ],
            "ENT Initial Consultation" =>
            [
                "Chronic sinus congestion and post-nasal drip for over three months.",
                "Persistent ringing (tinnitus) in the left ear for two weeks.",
                "Recurring sore throat and difficulty swallowing, especially in the mornings.",
                "Evaluation of chronic hoarseness and voice changes.",
                "Frequent nosebleeds (epistaxis) occurring several times a week.",
            ],
            "Hearing Loss Assessment" =>
            [
                "Noticed gradual hearing loss in both ears, especially in noisy environments.",
                "Audiometry requested due to sudden muffled hearing in the right ear.",
                "Routine hearing test; family members complain the TV is too loud.",
                "Follow-up hearing assessment after recovering from a middle ear infection.",
                "Evaluating suitability for hearing aids due to progressive age-related hearing loss.",
            ],
            "Neurological Evaluation" =>
            [
                "Experiencing frequent episodes of tingling and numbness in the hands and feet.",
                "Seeking evaluation for recurring migraine headaches with visual aura.",
                "Evaluation of mild memory loss and occasional confusion noted by family.",
                "Experiencing unexplained tremors in the right hand when resting.",
                "Assessment of persistent dizziness and loss of balance while walking.",
            ],
            "Brain MRI & Neuro-imaging Review" =>
            [
                "Reviewing results of recent brain MRI to investigate chronic headaches.",
                "Follow-up discussion of neuro-imaging findings for suspected multiple sclerosis.",
                "Reviewing MRI scan after a mild concussion to ensure recovery.",
                "Consultation to discuss brain scan results regarding pituitary microadenoma.",
                "Neuro-imaging review to monitor pre-existing benign cyst.",
            ],
            "Psychiatric Initial Intake" =>
            [
                "Experiencing persistent low mood, lack of energy, and loss of interest in activities.",
                "Severe anxiety and frequent panic attacks affecting daily work performance.",
                "Evaluation for suspected adult ADHD, having difficulty focusing.",
                "Experiencing sleep disturbances, racing thoughts, and high stress levels.",
                "Seeking evaluation for emotional instability and frequent mood swings.",
            ],
            "Mental Health Crisis Evaluation" =>
            [
                "Urgent assessment requested due to acute depressive state and feelings of hopelessness.",
                "Experiencing severe panic and high anxiety levels following a traumatic event.",
                "Seeking immediate help for overwhelming stress and inability to cope.",
                "Crisis evaluation due to acute insomnia and high agitation.",
                "Urgent evaluation for sudden worsening of bipolar disorder symptoms.",
            ],
            "Therapeutic Medication Follow-up" =>
            [
                "Routine review of antidepressant efficacy and side effects. Sleep is improving.",
                "Follow-up to monitor ADHD medication dose response and concentration levels.",
                "Reviewing progress on anti-anxiety medication; discussing dosage adjustment.",
                "Routine checkup for mood stabilizer medication; requesting blood test review.",
                "Evaluating response to new sleep medication; experiencing mild morning grogginess.",
            ],
            "Urological Initial Consultation" =>
            [
                "Experiencing frequent urination and burning sensation during urination.",
                "Difficulty initiating urination and weak urine flow for several weeks.",
                "Evaluation for recurrent kidney stones and lower back pain.",
                "Consultation regarding suspected erectile dysfunction and low libido.",
                "Evaluating occasional blood in the urine (hematuria).",
            ],
            "Prostate Cancer Prevention Screening" =>
            [
                "Annual prostate screening; requesting PSA blood test and physical exam.",
                "Routine urological checkup for prostate health due to family history.",
                "Prostate checkup; experiencing mild nocturia (waking up to urinate).",
                "Follow-up on slightly elevated PSA level from a previous screening.",
                "Annual preventive screening for prostate health.",
            ],
            "Oncology Initial Consultation" =>
            [
                "Referred by surgeon to discuss treatment options after recent biopsy results.",
                "Initial consultation to discuss chemotherapy plan for diagnosed breast cancer.",
                "Seeking a second opinion on lung nodule biopsy findings.",
                "Initial intake to discuss treatment strategy for colon cancer diagnosis.",
                "Consultation to review staging and next steps for newly diagnosed lymphoma.",
            ],
            "Chemotherapy Tolerance Checkup" =>
            [
                "Pre-chemo checkup; reviewing CBC results to confirm platelet counts are sufficient.",
                "Follow-up to assess nausea and fatigue levels after the second chemo cycle.",
                "Evaluating neuropathy symptoms in fingers and toes post-chemotherapy.",
                "Routine checkup to monitor kidney and liver function before next infusion.",
                "Assessing overall tolerance and weight stability during active chemotherapy.",
            ],
            "Endocrine Disorders Intake" =>
            [
                "Experiencing unexplained weight gain, dry skin, and extreme cold sensitivity.",
                "Evaluation for suspected thyroid nodule discovered during a physical exam.",
                "Seeking diagnostic assessment for chronic fatigue and hormonal imbalance.",
                "Evaluation of suspected adrenal insufficiency or pituitary dysfunction.",
                "Consultation for irregular hormone levels in recent laboratory results.",
            ],
            "Diabetes Mellitus Follow-up" =>
            [
                "Reviewing HbA1c results (currently 7.2%) and adjusting insulin doses.",
                "Routine diabetes checkup; reviewing daily blood glucose logs and diet.",
                "Evaluating diabetic neuropathy symptoms and conducting routine foot exam.",
                "Follow-up to adjust metformin dosage due to gastrointestinal sensitivity.",
                "Routine checkup; discussing continuous glucose monitor (CGM) reports.",
            ],
            "Gastroenterology Intake" =>
            [
                "Chronic abdominal pain and irregular bowel habits for several months.",
                "Seeking evaluation for persistent heartburn and acid reflux symptoms.",
                "Referred for a colonoscopy screening due to family history of colon polyps.",
                "Experiencing unexplained weight loss and frequent bouts of diarrhea.",
                "Evaluation of persistent nausea and upper abdominal discomfort.",
            ],
            "Chronic Digestive Disease Review" =>
            [
                "Routine follow-up for Crohn's disease; symptoms are currently in remission.",
                "Review of dietary management and medication for Irritable Bowel Syndrome (IBS).",
                "Follow-up for chronic gastritis; assessing efficacy of proton pump inhibitors.",
                "Review of ulcerative colitis symptoms and renewal of maintenance therapy.",
                "Monitoring liver function and symptoms of chronic fatty liver disease.",
            ],
            "Pulmonology Comprehensive Exam" =>
            [
                "Experiencing persistent shortness of breath during routine walking.",
                "Chronic cough lasting for over two months; seeking diagnostic testing.",
                "Spirometry and comprehensive lung function exam for suspected asthma.",
                "Evaluating chronic wheezing and chest tightness in a former smoker.",
                "Initial pulmonology consult to investigate abnormal chest X-ray findings.",
            ],
            "Asthma & COPD Control Check" =>
            [
                "Routine asthma control review; using rescue inhaler less than twice a week.",
                "COPD follow-up; assessing oxygen levels and review of daily maintenance inhalers.",
                "Asthma checkup; symptoms worsening during high pollen season.",
                "Review of COPD management plan and prescription renewal.",
                "Asthma control test and spirometry review post-respiratory infection.",
            ],
            _ => throw new InvalidOperationException(
                $"No patient notes defined for appointment type: '{apptTypeName}'"
            ),
        };

        return notes[index % notes.Length];
    }

    private static string GetReceptionistNotes(AppointmentStatus status, int index)
    {
        string[] notes = status switch
        {
            AppointmentStatus.Completed =>
            [
                "Identity was verified. Co-payment was processed via credit card. Patient was directed to the wait room.",
                "Patient checked in early. Insurance eligibility was verified online. Copay was collected.",
                "Vitals were recorded by the nurse. Copay was waived under the premium plan.",
                "Personal details were verified. Patient brought the requested lab test results.",
                "Identity document was scanned and updated in the system.",
            ],

            AppointmentStatus.CheckedIn =>
            [
                "Checked in. Patient waiting in Room A. Doctor notified.",
                "Checked in. Identity verified. Patient brought recent imaging documents.",
                "Patient arrived and checked in. Confirmed billing information.",
                "Checked in. Waiting for doctor to call them in.",
                "Checked in. Copay processed. Patient seated in reception.",
            ],
            AppointmentStatus.InProgress =>
            [
                "Checked in and sent to consultation room. Doctor active.",
                "Vitals taken. Patient entered doctor office for consultation.",
                "Consultation started. Patient with doctor.",
                "Checked in. Consultation currently in progress.",
                "Active consultation. Patient checked in on time.",
            ],
            _ => throw new InvalidOperationException(
                $"No receptionist notes defined for appointment status: '{status}'"
            ),
        };

        return notes[index % notes.Length];
    }

    private static string GetCancellationReason(int index)
    {
        string[] reasons =
        [
            "Work schedule conflict",
            "Feeling better, no longer need consultation",
            "Personal emergency",
            "Found another doctor sooner",
            "Forgot about the appointment",
            "Family emergency",
            "Travel plans changed",
            "Financial reasons",
            "Weather conditions",
            "Decided to reschedule for next month",
        ];
        return reasons[index % reasons.Length];
    }

    private static string GetLateCancellationReason(int index)
    {
        string[] reasons =
        [
            "Traffic jam",
            "Car trouble on the way to the clinic",
            "Unexpected meeting at work ran late",
            "Child got sick suddenly at school",
            "Woke up with severe migraine, unable to drive",
            "Family emergency occurred just now",
            "Flight delay returning to the city",
            "Severe weather made driving unsafe",
            "Forgot the appointment time and realized too late",
            "Work crisis required immediate attention",
        ];
        return reasons[index % reasons.Length];
    }
}
