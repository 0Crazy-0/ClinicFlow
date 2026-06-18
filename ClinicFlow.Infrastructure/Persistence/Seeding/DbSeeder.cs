using System.Collections.Immutable;
using Bogus;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence.Seeding.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Seeding;

/// <summary>
/// Handles database seeding for local development environments using Bogus to generate realistic mock data.
/// </summary>
public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context, TimeProvider timeProvider) =>
        SeedInternal(context, timeProvider).GetAwaiter().GetResult();

    public static Task SeedAsync(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default
    ) => SeedInternal(context, timeProvider, cancellationToken);

    private static async Task SeedInternal(
        ApplicationDbContext context,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default
    )
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var seederContext = new SeederContext();

        Randomizer.Seed = new Random(42);

        var specialties = await SeedMedicalSpecialtiesAsync(context, cancellationToken);
        var templates = await SeedClinicalFormTemplatesAsync(context, cancellationToken);
        var appointmentTypes = await SeedAppointmentTypeDefinitionsAsync(
            context,
            templates,
            specialties,
            cancellationToken
        );

        var (doctorUsers, patientUsers) = await SeedUsersAsync(
            context,
            seederContext,
            timeProvider,
            cancellationToken
        );

        var doctors = await SeedDoctorsAsync(
            context,
            doctorUsers,
            specialties,
            seederContext,
            cancellationToken
        );

        var patients = await SeedPatientsAsync(
            context,
            patientUsers,
            seederContext,
            timeProvider,
            cancellationToken
        );

        var schedules = await SeedSchedulesAsync(context, doctors, cancellationToken);
        var seedingArgs = new AppointmentSeedingArgs(
            patients,
            doctors,
            appointmentTypes,
            schedules,
            patientUsers
        );

        var appointments = await SeedAppointmentsAsync(
            context,
            seedingArgs,
            timeProvider,
            cancellationToken
        );

        await SeedMedicalRecordsAsync(
            context,
            appointments,
            appointmentTypes,
            specialties,
            cancellationToken
        );

        await SeedPatientPenaltiesAsync(context, appointments, cancellationToken);
    }

    private static async Task<IReadOnlyList<MedicalSpecialty>> SeedMedicalSpecialtiesAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken
    )
    {
        var specialtyData = new[]
        {
            // (name, durationMinutes, cancellationHours)
            ("General Medicine", 20, 12),
            ("Pediatrics", 20, 12),
            ("Cardiology", 45, 48),
            ("Dermatology", 15, 12),
            ("Gynaecology", 30, 24),
            ("Ophthalmology", 20, 12),
            ("Orthopedics", 40, 24),
            ("Otolaryngology", 20, 12),
            ("Neurology", 45, 48),
            ("Psychiatry", 60, 48),
            ("Urology", 30, 24),
            ("Oncology", 60, 72),
            ("Endocrinology", 40, 48),
            ("Gastroenterology", 35, 24),
            ("Pulmonology", 30, 24),
        };

        var specialties = specialtyData
            .Select(s =>
                MedicalSpecialty.Create(
                    s.Item1,
                    $"Specialty for {s.Item1} care and clinical consultation.",
                    s.Item2,
                    s.Item3
                )
            )
            .ToList();

        await context.MedicalSpecialties.AddRangeAsync(specialties, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return specialties;
    }

    private static async Task<IReadOnlyList<ClinicalFormTemplate>> SeedClinicalFormTemplatesAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken
    )
    {
        var templates = ClinicalFormTemplateData.GetTemplates();

        await context.ClinicalFormTemplates.AddRangeAsync(templates, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return templates;
    }

    private static async Task<
        IReadOnlyList<AppointmentTypeDefinition>
    > SeedAppointmentTypeDefinitionsAsync(
        ApplicationDbContext context,
        IReadOnlyList<ClinicalFormTemplate> templates,
        IReadOnlyList<MedicalSpecialty> specialties,
        CancellationToken cancellationToken
    )
    {
        var templatesByCode = templates.ToDictionary(t => t.Code);
        var specialtiesByName = specialties.ToDictionary(s => s.Name);
        var apptTypeData = AppointmentTypeDefinitionData.GetSeedItems();
        var appointmentTypes = new List<AppointmentTypeDefinition>();

        foreach (var data in apptTypeData)
        {
            var apptType = AppointmentTypeDefinition.Create(
                data.Category,
                data.Name,
                data.Desc,
                TimeSpan.FromMinutes(data.Duration),
                data.AgePolicy
            );

            if (templatesByCode.TryGetValue(data.TemplateCode, out var template))
                apptType.AddRequiredTemplate(template);

            if (specialtiesByName.TryGetValue(data.SpecialtyName, out var specialty))
                apptType.RestrictToSpecialties([specialty.Id]);

            if (AppointmentTypeDefinitionData.TypesToDeactivate.Contains(apptType.Name))
                apptType.Deactivate();

            appointmentTypes.Add(apptType);
        }

        await context.AppointmentTypes.AddRangeAsync(appointmentTypes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return appointmentTypes;
    }

    private static async Task<(
        IReadOnlyList<User> DoctorUsers,
        IReadOnlyList<User> PatientUsers
    )> SeedUsersAsync(
        ApplicationDbContext context,
        SeederContext seederContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var users = new List<User>();
        var doctorUsers = new List<User>();
        var patientUsers = new List<User>();
        var refTime = timeProvider.GetUtcNow().UtcDateTime;

        // Admins (3)
        var adminLoginOffsets = new (int Days, int Hours)[] { (1, 3), (4, 11), (2, 17) };

        for (int i = 0; i < 3; i++)
        {
            var user = User.Create(
                EmailAddress.Create($"admin{i + 1}@clinicflow.com"),
                SeederContext.HashedPassword,
                PhoneNumber.Create(seederContext.GenerateUniquePhoneNumber()),
                UserRole.Admin
            );
            user.MarkPhoneAsVerified(true);
            user.RecordLogin(
                refTime.AddDays(-adminLoginOffsets[i].Days).AddHours(-adminLoginOffsets[i].Hours)
            );
            users.Add(user);
        }

        /// Receptionists (8)
        var receptionistLoginOffsets = new (int Days, int Hours)[]
        {
            (1, 8),
            (3, 14),
            (1, 20),
            (5, 9),
            (2, 16),
            (7, 6),
            (1, 11),
            (4, 19),
        };

        for (int i = 0; i < 8; i++)
        {
            var user = User.Create(
                EmailAddress.Create($"receptionist{i + 1}@clinicflow.com"),
                SeederContext.HashedPassword,
                PhoneNumber.Create(seederContext.GenerateUniquePhoneNumber()),
                UserRole.Receptionist
            );
            user.MarkPhoneAsVerified(true);
            user.RecordLogin(
                refTime
                    .AddDays(-receptionistLoginOffsets[i].Days)
                    .AddHours(-receptionistLoginOffsets[i].Hours)
            );
            users.Add(user);
        }

        // Doctors (35)
        for (int i = 0; i < 35; i++)
        {
            var user = User.Create(
                EmailAddress.Create($"doctor{i + 1}@clinicflow.com"),
                SeederContext.HashedPassword,
                PhoneNumber.Create(seederContext.GenerateUniquePhoneNumber()),
                UserRole.Doctor
            );

            var profile = _doctorVariantCycle[i % _doctorVariantCycle.Length];

            if (profile.IsPhoneVerified)
                user.MarkPhoneAsVerified(true);

            if (profile.DaysAgoLastLogin.HasValue)
                user.RecordLogin(
                    refTime.AddDays(-profile.DaysAgoLastLogin.Value).AddHours(-(i % 24))
                );

            users.Add(user);
            doctorUsers.Add(user);
        }

        // Patients (120: 115 active, 5 inactive)
        for (int i = 0; i < 120; i++)
        {
            var user = User.Create(
                EmailAddress.Create($"patient{i + 1}@clinicflow.com"),
                SeederContext.HashedPassword,
                PhoneNumber.Create(seederContext.GenerateUniquePhoneNumber()),
                UserRole.Patient
            );

            ApplyPatientVariety(user, i, refTime);

            if (i >= 115)
                user.Deactivate();

            users.Add(user);
            patientUsers.Add(user);
        }

        await context.Users.AddRangeAsync(users, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return (doctorUsers, patientUsers);
    }

    private static async Task<IReadOnlyList<Doctor>> SeedDoctorsAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> doctorUsers,
        IReadOnlyList<MedicalSpecialty> specialties,
        SeederContext seederContext,
        CancellationToken cancellationToken
    )
    {
        var assignableSpecialties = specialties
            .Where(s => !SeederContext.SpecialtiesToDeactivate.Contains(s.Name))
            .ToList();

        var doctors = new List<Doctor>();

        for (int i = 0; i < 35; i++)
        {
            var doctorUser = doctorUsers[i];
            var specialty = assignableSpecialties[i % assignableSpecialties.Count];
            var floor = (i % 8) + 1;
            var roomNumber = i + 1;
            var doctor = Doctor.Create(
                doctorUser.Id,
                PersonName.Create(seederContext.Faker.Name.FullName()),
                MedicalLicenseNumber.Create($"CMP-{10000 + i}"),
                specialty.Id,
                $"Experienced specialist in {specialty.Name}.",
                ConsultationRoom.Create(
                    roomNumber,
                    $"Consultorio {roomNumber} - Piso {floor}",
                    floor
                )
            );

            if (i < 3)
                doctor.Suspend();

            doctors.Add(doctor);
        }

        await context.Doctors.AddRangeAsync(doctors, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        foreach (
            var specialty in specialties.Where(s =>
                SeederContext.SpecialtiesToDeactivate.Contains(s.Name)
            )
        )
        {
            var hasActiveDoctors = doctors.Any(d =>
                d.MedicalSpecialtyId == specialty.Id && !d.IsDeleted
            );
            specialty.Deactivate(hasActiveDoctors);
        }

        await context.SaveChangesAsync(cancellationToken);

        return doctors;
    }

    private static async Task<IReadOnlyList<Patient>> SeedPatientsAsync(
        ApplicationDbContext context,
        IReadOnlyList<User> patientUsers,
        SeederContext seederContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var patients = new List<Patient>();
        var refTime = timeProvider.GetUtcNow().UtcDateTime;
        var faker = seederContext.Faker;

        string[] bloodTypes = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];
        string[] allergyPool =
        [
            "None",
            "Penicillin",
            "Aspirin",
            "Ibuprofen",
            "Latex",
            "Pollen",
            "Dust",
            "Shellfish",
            "Peanuts",
            "Tree Nuts",
            "Milk",
            "Eggs",
            "Wheat",
            "Soy",
            "Fish",
            "Mold",
            "Animal Dander",
            "Sulfa Drugs",
        ];
        string[] conditionPool =
        [
            "None",
            "Hypertension",
            "Diabetes",
            "Asthma",
            "Arthritis",
            "Obesity",
            "Anxiety",
            "Depression",
            "Hypothyroidism",
            "Hyperthyroidism",
            "Chronic Back Pain",
            "Migraine",
            "GERD",
            "Sleep Apnea",
            "High Cholesterol",
            "Anemia",
            "Osteoporosis",
            "Chronic Kidney Disease",
        ];

        // Self
        for (int i = 0; i < 105; i++)
        {
            var pUser = patientUsers[i];
            var patient = Patient.CreateSelf(
                pUser.Id,
                PersonName.Create(faker.Name.FullName()),
                DateOnly.FromDateTime(faker.Date.Past(21, refTime.AddYears(-18))),
                refTime
            );
            patient.UpdateMedicalProfile(
                BloodType.Create(faker.PickRandom(bloodTypes)),
                faker.PickRandom(allergyPool),
                faker.PickRandom(conditionPool)
            );
            patient.UpdateEmergencyContact(
                EmergencyContact.Create(
                    faker.Name.FullName(),
                    seederContext.GenerateUniquePhoneNumber()
                )
            );
            patients.Add(patient);
        }

        // Self old
        for (int i = 105; i < 120; i++)
        {
            var pUser = patientUsers[i];
            var patient = Patient.CreateSelf(
                pUser.Id,
                PersonName.Create(faker.Name.FullName()),
                DateOnly.FromDateTime(faker.Date.Past(40, refTime.AddYears(-40))),
                refTime
            );
            patient.UpdateMedicalProfile(
                BloodType.Create(faker.PickRandom(bloodTypes)),
                faker.PickRandom(allergyPool),
                faker.PickRandom(conditionPool)
            );
            patient.UpdateEmergencyContact(
                EmergencyContact.Create(
                    faker.Name.FullName(),
                    seederContext.GenerateUniquePhoneNumber()
                )
            );
            patients.Add(patient);
        }

        // Family Members (80 dependents)
        var dependentsToCreate = new List<PatientRelationship>();
        for (int i = 0; i < 25; i++)
            dependentsToCreate.Add(PatientRelationship.Child);
        for (int i = 0; i < 20; i++)
            dependentsToCreate.Add(PatientRelationship.Spouse);
        for (int i = 0; i < 15; i++)
            dependentsToCreate.Add(PatientRelationship.Parent);
        for (int i = 0; i < 10; i++)
            dependentsToCreate.Add(PatientRelationship.Sibling);
        for (int i = 0; i < 10; i++)
            dependentsToCreate.Add(PatientRelationship.Other);

        for (int i = 0; i < dependentsToCreate.Count; i++)
        {
            var pUser = patientUsers[i % 120];
            var relationship = dependentsToCreate[i];
            int age = relationship switch
            {
                PatientRelationship.Child => faker.Random.Number(1, 17),
                PatientRelationship.Parent => faker.Random.Number(50, 80),
                PatientRelationship.Spouse => faker.Random.Number(25, 45),
                _ => faker.Random.Number(18, 65),
            };
            var patient = Patient.CreateFamilyMember(
                pUser.Id,
                PersonName.Create(faker.Name.FullName()),
                relationship,
                DateOnly.FromDateTime(faker.Date.Past(age, refTime.AddYears(-age))),
                refTime
            );
            patient.UpdateMedicalProfile(
                BloodType.Create(faker.PickRandom(bloodTypes)),
                faker.PickRandom(allergyPool),
                faker.PickRandom(conditionPool)
            );
            patient.UpdateEmergencyContact(
                EmergencyContact.Create(
                    faker.Name.FullName(),
                    seederContext.GenerateUniquePhoneNumber()
                )
            );
            patients.Add(patient);
        }

        // Close 5 self patient accounts.
        for (int i = 100; i < 105; i++)
            patients[i].CloseAccount(hasPendingAppointments: false);

        // Remove 8 family member patients.
        for (int i = 120; i < 128; i++)
            patients[i].RemoveFamilyMember(patients[i].UserId);

        await context.Patients.AddRangeAsync(patients, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return patients;
    }

    private static async Task<IReadOnlyList<Schedule>> SeedSchedulesAsync(
        ApplicationDbContext context,
        IReadOnlyList<Doctor> doctors,
        CancellationToken cancellationToken
    )
    {
        var faker = new Faker();
        var schedules = new List<Schedule>();
        var shiftPool = new (TimeOnly Start, TimeOnly End)[]
        {
            (new TimeOnly(6, 0), new TimeOnly(12, 0)),
            (new TimeOnly(7, 0), new TimeOnly(13, 0)),
            (new TimeOnly(7, 0), new TimeOnly(16, 0)),
            (new TimeOnly(8, 0), new TimeOnly(14, 0)),
            (new TimeOnly(8, 0), new TimeOnly(17, 0)),
            (new TimeOnly(9, 0), new TimeOnly(15, 0)),
            (new TimeOnly(12, 0), new TimeOnly(18, 0)),
            (new TimeOnly(13, 0), new TimeOnly(19, 0)),
            (new TimeOnly(14, 0), new TimeOnly(20, 0)),
            (new TimeOnly(15, 0), new TimeOnly(21, 0)),
        };

        var weekdays = new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
        };

        for (int d = 0; d < 35; d++)
        {
            var doctor = doctors[d];

            for (int w = 0; w < 5; w++)
            {
                var (start, end) = faker.PickRandom(shiftPool);
                var timeRange = TimeRange.Create(start, end);
                var schedule = Schedule.Create(doctor.Id, weekdays[w], timeRange);

                if (faker.Random.Bool(0.15f))
                    schedule.Deactivate();

                schedules.Add(schedule);
            }
        }

        await context.Schedules.AddRangeAsync(schedules, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return schedules;
    }

    private static async Task<IReadOnlyList<Appointment>> SeedAppointmentsAsync(
        ApplicationDbContext context,
        AppointmentSeedingArgs args,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var baseDate = timeProvider.GetUtcNow().UtcDateTime.Date;
        var generator = new AppointmentGenerator(args, baseDate);
        var appointments = Enumerable.Range(0, 500).Select(generator.Generate).ToList();

        await context.Appointments.AddRangeAsync(appointments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return appointments;
    }

    private static async Task SeedMedicalRecordsAsync(
        ApplicationDbContext context,
        IReadOnlyList<Appointment> appointments,
        IReadOnlyList<AppointmentTypeDefinition> appointmentTypes,
        IReadOnlyList<MedicalSpecialty> specialties,
        CancellationToken cancellationToken
    )
    {
        var faker = new Faker();
        var medicalRecords = new List<MedicalRecord>();
        var completedAppointments = appointments
            .Where(a => a.Status is AppointmentStatus.Completed)
            .ToList();

        var specialtyById = specialties.ToDictionary(s => s.Id, s => s.Name);
        var apptTypeById = appointmentTypes.ToDictionary(t => t.Id);

        for (int i = 0; i < 250; i++)
        {
            var appt = completedAppointments[i];

            var record = MedicalRecord.Create(
                appt.PatientId,
                appt.DoctorId,
                appt.Id,
                ResolveChiefComplaint(appt, apptTypeById, specialtyById, faker)
            );

            var templateCode = ResolveTemplateCode(appt, apptTypeById);
            var jsonPayload = ClinicalDetailSampleData.GetSamplePayload(templateCode, i);
            var detail = DynamicClinicalDetail.Create(templateCode, jsonPayload);

            record.AddClinicalDetail(detail);
            medicalRecords.Add(record);
        }
        await context.MedicalRecords.AddRangeAsync(medicalRecords, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPatientPenaltiesAsync(
        ApplicationDbContext context,
        IReadOnlyList<Appointment> appointments,
        CancellationToken cancellationToken
    )
    {
        var penalties = new List<PatientPenalty>();
        var infractionAppts = appointments
            .Where(a => a.Status is AppointmentStatus.LateCancellation or AppointmentStatus.NoShow)
            .OrderBy(a => a.ScheduledDate)
            .ToList();

        var infractionsByPatient = infractionAppts.GroupBy(a => a.PatientId).ToList();

        foreach (var group in infractionsByPatient)
        {
            var patientId = group.Key;
            var patientPenalties = new List<PatientPenalty>();

            foreach (var appt in group)
            {
                var reason =
                    appt.Status is AppointmentStatus.LateCancellation
                        ? PenaltyReasons.LateCancellation
                        : PenaltyReasons.NoShow;

                var newPenalties = PatientPenaltyService.ApplyPenalty(
                    patientId,
                    patientPenalties.AsReadOnly(),
                    appt.Id,
                    reason,
                    appt.ScheduledDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                );

                patientPenalties.AddRange(newPenalties);
            }

            penalties.AddRange(patientPenalties);
        }

        // Add removed warnings for patients who had penalties resolved by staff.
        for (int i = 0; i < Math.Min(10, infractionsByPatient.Count); i++)
        {
            var group = infractionsByPatient[i];
            var warning = PatientPenalty.CreateAutomaticWarning(
                group.Key,
                group.First().Id,
                PenaltyReasons.LateCancellation
            );
            warning.Remove();
            penalties.Add(warning);
        }

        await context.PatientPenalties.AddRangeAsync(penalties, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string ResolveTemplateCode(
        Appointment appointment,
        Dictionary<Guid, AppointmentTypeDefinition> apptTypeById
    )
    {
        if (
            apptTypeById.TryGetValue(appointment.AppointmentTypeId, out var apptType)
            && apptType.RequiredTemplates.Count > 0
        )
            return apptType.RequiredTemplates.First().Code;

        return "GEN_INTAKE_V1";
    }

    private static void ApplyPatientVariety(User user, int index, DateTime refTime)
    {
        var profile = _variantCycle[index % _variantCycle.Length];

        if (profile.IsPhoneVerified)
            user.MarkPhoneAsVerified(true);

        if (profile.DaysAgoLastLogin.HasValue)
            user.RecordLogin(
                refTime.AddDays(-profile.DaysAgoLastLogin.Value).AddHours(-(index % 24))
            );

        for (int f = 0; f < profile.FailedLoginAttempts; f++)
            user.RecordFailedLogin(refTime.AddHours(-(profile.FailedLoginAttempts - f)));
    }

    private static string ResolveChiefComplaint(
        Appointment appointment,
        Dictionary<Guid, AppointmentTypeDefinition> apptTypeById,
        Dictionary<Guid, string> specialtyById,
        Faker faker
    )
    {
        var complaintsBySpecialty = ChiefComplaintData.GetBySpecialty();

        if (apptTypeById.TryGetValue(appointment.AppointmentTypeId, out var apptType))
        {
            var specialtyId = apptType.AllowedSpecialtyIds.FirstOrDefault();
            if (
                specialtyId != Guid.Empty
                && specialtyById.TryGetValue(specialtyId, out var specialtyName)
                && complaintsBySpecialty.TryGetValue(specialtyName, out var complaints)
            )
                return faker.PickRandom(complaints);
        }

        return faker.PickRandom(complaintsBySpecialty["General Medicine"]);
    }

    private static readonly UserVariantProfile[] _doctorVariantCycle =
    [
        new(true, 0, 1),
        new(true, 0, 3),
        new(false, 0, 5),
        new(true, 0, 2),
        new(true, 0, null),
        new(true, 0, 7),
        new(false, 0, 10),
        new(true, 0, 4),
    ];

    private readonly record struct UserVariantProfile(
        bool IsPhoneVerified,
        int FailedLoginAttempts,
        int? DaysAgoLastLogin
    );

    private static readonly UserVariantProfile[] _variantCycle =
    [
        new(true, 0, null),
        new(true, 0, 3),
        new(true, 1, 5),
        new(true, 3, 7),
        new(true, 5, null),
        new(false, 0, null),
        new(true, 0, 1),
        new(true, 2, 10),
        new(true, 0, 2),
        new(true, 5, 4),
        new(true, 0, 15),
        new(false, 1, 6),
        new(true, 0, 8),
        new(true, 4, 12),
        new(true, 0, 9),
    ];

    private sealed class SeederContext
    {
        public static readonly ImmutableHashSet<string> SpecialtiesToDeactivate =
            ImmutableHashSet.Create("Orthopedics", "Neurology");

        public const string HashedPassword = "hashed_password_123";
        public Faker Faker { get; } = new();
        public HashSet<string> UsedPhoneNumbers { get; } = [];

        public string GenerateUniquePhoneNumber()
        {
            while (true)
            {
                var num = $"+519{Faker.Random.Number(10000000, 99999999)}";
                if (UsedPhoneNumbers.Add(num))
                    return num;
            }
        }
    }
}
