namespace ClinicFlow.Domain.Common;

/// <summary>
/// Contains all the standardized error codes used by Domain Exceptions across the system.
/// </summary>
public static class DomainErrors
{
    public static class General
    {
        public const string RequiredFieldNull = "REQUIRED_FIELD_NULL";
        public const string NotFound = "ENTITY_NOT_FOUND";
    }

    public static class Validation
    {
        public const string ValueRequired = "VALUE_REQUIRED";
        public const string InvalidValue = "INVALID_VALUE";
        public const string ValueTooShort = "VALUE_TOO_SHORT";
        public const string ValueCannotBeNegative = "VALUE_CANNOT_BE_NEGATIVE";
        public const string ValueMustBePositive = "VALUE_MUST_BE_POSITIVE";
        public const string ValueCannotBeInFuture = "VALUE_CANNOT_BE_IN_FUTURE";
        public const string ValueMustBeInFuture = "VALUE_MUST_BE_IN_FUTURE";
        public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
        public const string InvalidPhoneFormat = "INVALID_PHONE_FORMAT";
        public const string InvalidBloodType = "INVALID_BLOOD_TYPE";
        public const string InvalidFormat = "INVALID_FORMAT";
        public const string StartTimeMustBeBeforeEndTime = "START_TIME_MUST_BE_BEFORE_END_TIME";
        public const string EndTimeMustBeAfterStartTime = "END_TIME_MUST_BE_AFTER_START_TIME";
        public const string InvalidDateRange = "INVALID_DATE_RANGE";
        public const string InvalidEnumValue = "INVALID_ENUM_VALUE";
        public const string ValueTooLong = "VALUE_TOO_LONG";
        public const string ValueExceedsMaximum = "VALUE_EXCEEDS_MAXIMUM";
    }

    public static class Appointment
    {
        public const string CannotCancel = "CANCELLATION_NOT_ALLOWED";
        public const string UnauthorizedCancellation = "CANCELLATION_UNAUTHORIZED";
        public const string MissingCancellationReason = "MISSING_CANCELLATION_REASON";
        public const string CannotReschedule = "RESCHEDULING_NOT_ALLOWED";
        public const string CannotMarkNoShow = "NO_SHOW_NOT_ALLOWED";
        public const string UnauthorizedNoShow = "NO_SHOW_UNAUTHORIZED";
        public const string Conflict = "APPOINTMENT_CONFLICT";
        public const string DataMismatch = "APPOINTMENT_DATA_MISMATCH";
        public const string UnauthorizedScheduling = "SCHEDULING_UNAUTHORIZED";
        public const string CannotCheckIn = "CHECK_IN_NOT_ALLOWED";
        public const string CannotStart = "START_NOT_ALLOWED";
        public const string CannotComplete = "COMPLETE_NOT_ALLOWED";
        public const string UnauthorizedDoctor = "UNAUTHORIZED_DOCTOR";
    }

    public static class AppointmentType
    {
        public const string TemplateAlreadyRequired = "TEMPLATE_ALREADY_REQUIRED";
        public const string InvalidAgeRange = "INVALID_AGE_RANGE";
        public const string MinimumAgeNotMet = "MINIMUM_AGE_NOT_MET";
        public const string MaximumAgeExceeded = "MAXIMUM_AGE_EXCEEDED";
        public const string LegalGuardianRequired = "LEGAL_GUARDIAN_REQUIRED";
    }

    public static class Schedule
    {
        public const string InvalidDayOfWeek = "INVALID_DAY_OF_WEEK";
        public const string InvalidTimeRange = "INVALID_TIME_RANGE";
        public const string DoctorNotAvailable = "DOCTOR_NOT_AVAILABLE";
        public const string ScheduleAlreadyExists = "SCHEDULE_ALREADY_EXISTS";
        public const string AlreadyInactive = "SCHEDULE_ALREADY_INACTIVE";
    }

    public static class Patient
    {
        public const string Blocked = "PATIENT_BLOCKED";
        public const string CannotBeSelf = "INVALID_FAMILY_RELATIONSHIP";
        public const string ProfileIncomplete = "PATIENT_PROFILE_INCOMPLETE";
    }

    public static class Penalty
    {
        public const string AlreadyRemoved = "PENALTY_ALREADY_REMOVED";
    }

    public static class MedicalEncounter
    {
        public const string DoctorMismatch = "DOCTOR_MISMATCH";
        public const string AppointmentMismatch = "APPOINTMENT_MISMATCH";
        public const string CodeMismatch = "TEMPLATE_CODE_MISMATCH";
        public const string MissingPayload = "MISSING_PAYLOAD";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string MissingRequiredTemplate = "MISSING_REQUIRED_TEMPLATE";
        public const string DetailAlreadyExists = "CLINICAL_DETAIL_ALREADY_EXISTS";
    }
}
