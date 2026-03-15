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
        public const string ValueTooShort = "VALUE_TOO_SHORT";
        public const string ValueCannotBeNegative = "VALUE_CANNOT_BE_NEGATIVE";
        public const string ValueMustBePositive = "VALUE_MUST_BE_POSITIVE";
        public const string ValueCannotBeInFuture = "VALUE_CANNOT_BE_IN_FUTURE";
        public const string ValueMustBeInFuture = "VALUE_MUST_BE_IN_FUTURE";
        public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
        public const string InvalidPhoneFormat = "INVALID_PHONE_FORMAT";
        public const string InvalidBloodType = "INVALID_BLOOD_TYPE";
    }

    public static class Appointment
    {
        public const string CannotCancel = "CANCELLATION_NOT_ALLOWED";
        public const string UnauthorizedCancellation = "CANCELLATION_UNAUTHORIZED";
        public const string MissingCancellationReason = "MISSING_CANCELLATION_REASON";
        public const string CannotConfirm = "CONFIRMATION_NOT_ALLOWED";
        public const string CannotReschedule = "RESCHEDULING_NOT_ALLOWED";
        public const string CannotMarkNoShow = "NO_SHOW_NOT_ALLOWED";
        public const string Conflict = "APPOINTMENT_CONFLICT";
    }

    public static class AppointmentType
    {
        public const string TemplateAlreadyRequired = "TEMPLATE_ALREADY_REQUIRED";
    }

    public static class Schedule
    {
        public const string InvalidDayOfWeek = "INVALID_DAY_OF_WEEK";
        public const string InvalidTimeRange = "INVALID_TIME_RANGE";
        public const string DoctorNotAvailable = "DOCTOR_NOT_AVAILABLE";
    }

    public static class Patient
    {
        public const string Blocked = "PATIENT_BLOCKED";
        public const string CannotBeSelf = "INVALID_FAMILY_RELATIONSHIP";
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
