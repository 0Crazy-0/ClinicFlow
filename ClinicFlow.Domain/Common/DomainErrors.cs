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
        public const string EndTimeMustBeAfterStartTime = "END_TIME_MUST_BE_AFTER_START_TIME";
        public const string InvalidDateRange = "INVALID_DATE_RANGE";
        public const string InvalidEnumValue = "INVALID_ENUM_VALUE";
        public const string ValueTooLong = "VALUE_TOO_LONG";
        public const string ValueExceedsMaximum = "VALUE_EXCEEDS_MAXIMUM";
        public const string DuplicateValues = "DUPLICATE_VALUES";
    }

    public static class Address
    {
        public const string InvalidLineFormat = "INVALID_ADDRESS_LINE_FORMAT";
        public const string InvalidPostalCode = "INVALID_POSTAL_CODE_FORMAT";
        public const string StateCountryMismatch = "ADDRESS_STATE_COUNTRY_MISMATCH";
        public const string StateNotInCatalog = "ADDRESS_STATE_NOT_IN_CATALOG";
        public const string CountryNotInCatalog = "ADDRESS_COUNTRY_NOT_IN_CATALOG";
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
        public const string UnauthorizedAccess = "APPOINTMENT_UNAUTHORIZED_ACCESS";
        public const string CannotCheckIn = "CHECK_IN_NOT_ALLOWED";
        public const string CannotStart = "START_NOT_ALLOWED";
        public const string CannotComplete = "COMPLETE_NOT_ALLOWED";
        public const string UnauthorizedDoctor = "UNAUTHORIZED_DOCTOR";
        public const string CannotReassign = "REASSIGNMENT_NOT_ALLOWED";
        public const string Duplicate = "DUPLICATE_APPOINTMENT";
        public const string CannotUpdateNotes = "UPDATE_NOTES_NOT_ALLOWED";
        public const string GuardianRequiresMinor = "GUARDIAN_REQUIRES_MINOR";
        public const string AdultHistoryRequiresAdult = "ADULT_HISTORY_REQUIRES_ADULT";
        public const string MinorHistoryRequiresMinor = "MINOR_HISTORY_REQUIRES_MINOR";
        public const string InvalidCompletionDate = "COMPLETION_DATE_BEFORE_APPOINTMENT_START";
        public const string CompletionBeforeActualStart = "COMPLETION_BEFORE_ACTUAL_START";
        public const string InvalidStartDate = "START_DATE_MISMATCH";
        public const string InvalidCheckInDate = "CHECK_IN_DATE_MISMATCH";
        public const string CheckInAfterEndTime = "CHECK_IN_AFTER_APPOINTMENT_END_TIME";
    }

    public static class Scheduling
    {
        public const string MissingClearance = "MISSING_SCHEDULING_CLEARANCE";
        public const string AmbiguousTimeSlot = "AMBIGUOUS_TIME_SLOT";
        public const string InvalidTimeSlot = "INVALID_TIME_SLOT";
    }

    public static class Reschedule
    {
        public const string MissingClearance = "MISSING_RESCHEDULING_CLEARANCE";
    }

    public static class AppointmentType
    {
        public const string NameAlreadyExists = "APPOINTMENT_TYPE_NAME_ALREADY_EXISTS";
        public const string AlreadyActive = "APPOINTMENT_TYPE_ALREADY_ACTIVE";
        public const string AlreadyInactive = "APPOINTMENT_TYPE_ALREADY_INACTIVE";
        public const string TemplateAlreadyRequired = "TEMPLATE_ALREADY_REQUIRED";
        public const string SpecialtyAlreadyAllowed = "SPECIALTY_ALREADY_ALLOWED";
        public const string CannotAddSpecialtyToGlobalType = "CANNOT_ADD_SPECIALTY_TO_GLOBAL_TYPE";
        public const string CannotRemoveSpecialtyFromGlobalType =
            "CANNOT_REMOVE_SPECIALTY_FROM_GLOBAL_TYPE";
        public const string AlreadyRestricted = "ALREADY_RESTRICTED";
        public const string RequiresAtLeastOneSpecialty = "REQUIRES_AT_LEAST_ONE_SPECIALTY";
        public const string AlreadyUnrestricted = "ALREADY_UNRESTRICTED";
        public const string SpecialtyNotFound = "SPECIALTY_NOT_FOUND";
        public const string TemplateNotFound = "TEMPLATE_NOT_FOUND";
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
        public const string UnauthorizedAccess = "PATIENT_UNAUTHORIZED_ACCESS";
        public const string PrimaryPatientRequired = "PRIMARY_PATIENT_REQUIRED";
        public const string BiologicalSexUnchanged = "BIOLOGICAL_SEX_UNCHANGED";
        public const string DataMismatch = "PATIENT_DATA_MISMATCH";
    }

    public static class Doctor
    {
        public const string InactiveProfileExists = "INACTIVE_DOCTOR_PROFILE_EXISTS";
        public const string AlreadyActive = "DOCTOR_ALREADY_ACTIVE";
        public const string AlreadySuspended = "DOCTOR_ALREADY_SUSPENDED";
    }

    public static class MedicalSpecialty
    {
        public const string NameAlreadyExists = "SPECIALTY_NAME_ALREADY_EXISTS";
        public const string AlreadyActive = "SPECIALTY_ALREADY_ACTIVE";
        public const string AlreadyInactive = "SPECIALTY_ALREADY_INACTIVE";
        public const string HasActiveDoctors = "SPECIALTY_HAS_ACTIVE_DOCTORS";
        public const string InvalidCancellationLimit = "INVALID_CANCELLATION_LIMIT";
        public const string InvalidEncounterDuration = "INVALID_ENCOUNTER_DURATION";
    }

    public static class ClinicalFormTemplate
    {
        public const string CodeAlreadyExists = "TEMPLATE_CODE_ALREADY_EXISTS";
        public const string NameAlreadyExists = "TEMPLATE_NAME_ALREADY_EXISTS";
        public const string AlreadyActive = "TEMPLATE_ALREADY_ACTIVE";
        public const string AlreadyInactive = "TEMPLATE_ALREADY_INACTIVE";
    }

    public static class User
    {
        public const string AccountInactive = "ACCOUNT_INACTIVE";
        public const string AccountLockedOut = "ACCOUNT_LOCKED_OUT";
        public const string CannotCloseAccountWithActiveFamilyMembers =
            "CANNOT_CLOSE_ACCOUNT_WITH_ACTIVE_FAMILY_MEMBERS";
        public const string AlreadyActive = "USER_ALREADY_ACTIVE";
        public const string AlreadyInactive = "USER_ALREADY_INACTIVE";
        public const string AlreadyActivated = "USER_ALREADY_ACTIVATED";
        public const string PasswordChangeNotAllowed = "PASSWORD_CHANGE_NOT_ALLOWED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    }

    public static class Penalty
    {
        public const string AlreadyRemoved = "PENALTY_ALREADY_REMOVED";
    }

    public static class MedicalRecord
    {
        public const string UnauthorizedAccess = "MEDICAL_RECORD_UNAUTHORIZED_ACCESS";
        public const string ProtectedByMinorConsent = "MEDICAL_RECORD_PROTECTED_BY_MINOR_CONSENT";
        public const string GuardianInitiatedTreatmentRequired =
            "GUARDIAN_INITIATED_TREATMENT_REQUIRED";
        public const string GuardianInitiatedTreatmentNotApplicable =
            "GUARDIAN_INITIATED_TREATMENT_NOT_APPLICABLE";
        public const string GuardianInvolvementFlagNotApplicable =
            "GUARDIAN_INVOLVEMENT_FLAG_NOT_APPLICABLE";
    }

    public static class MedicalEncounter
    {
        public const string DoctorMismatch = "DOCTOR_MISMATCH";
        public const string AppointmentMismatch = "APPOINTMENT_MISMATCH";
        public const string CodeMismatch = "TEMPLATE_CODE_MISMATCH";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string MissingRequiredTemplate = "MISSING_REQUIRED_TEMPLATE";
        public const string DetailAlreadyExists = "CLINICAL_DETAIL_ALREADY_EXISTS";
        public const string AppointmentNotInProgress = "APPOINTMENT_NOT_IN_PROGRESS";
        public const string AppointmentNotInProgressOrCompleted =
            "APPOINTMENT_NOT_IN_PROGRESS_OR_COMPLETED";
    }

    public static class FamilyMembership
    {
        public const string CannotBeSelf = "INVALID_FAMILY_RELATIONSHIP";
        public const string AlreadyTerminated = "FAMILY_MEMBERSHIP_ALREADY_TERMINATED";
        public const string PatientAlreadyHasActiveMembership =
            "PATIENT_ALREADY_HAS_ACTIVE_MEMBERSHIP";
        public const string PatientTooYoungForSelfMembership =
            "PATIENT_TOO_YOUNG_FOR_SELF_MEMBERSHIP";

        public const string CannotChangeAccessLevelOfSelf =
            "FAMILY_MEMBERSHIP_CANNOT_CHANGE_ACCESS_LEVEL_OF_SELF";
        public const string UnauthorizedAccessLevelChange =
            "FAMILY_MEMBERSHIP_UNAUTHORIZED_ACCESS_LEVEL_CHANGE";
        public const string UnauthorizedCategoryListChange =
            "FAMILY_MEMBERSHIP_UNAUTHORIZED_CATEGORY_LIST_CHANGE";
        public const string AccessLevelUnchanged = "FAMILY_MEMBERSHIP_ACCESS_LEVEL_UNCHANGED";
        public const string OwnerMustBeAdult = "OWNER_MUST_BE_ADULT";
        public const string MemberMustBeAdultToLeave = "MEMBER_MUST_BE_ADULT_TO_LEAVE";
        public const string MaxActiveFamilyMembersExceeded = "MAX_ACTIVE_FAMILY_MEMBERS_EXCEEDED";
        public const string CannotRemoveSelf = "CANNOT_REMOVE_SELF_MEMBERSHIP";
        public const string CannotLeaveSelf = "CANNOT_LEAVE_SELF_MEMBERSHIP";
        public const string CannotRemoveWithoutOwnSelf = "PATIENT_HAS_NO_OWN_SELF_MEMBERSHIP";
        public const string CanOnlyCloseSelfMembership = "CAN_ONLY_CLOSE_SELF_MEMBERSHIP";
        public const string CannotRemoveWithUpcomingAppointments =
            "CANNOT_REMOVE_WITH_UPCOMING_APPOINTMENTS";
        public const string OnlyRestrictedCanHaveCategoryList =
            "FAMILY_MEMBERSHIP_ONLY_RESTRICTED_CAN_HAVE_CATEGORY_LIST";
        public const string CategoryAlreadyAllowed = "FAMILY_MEMBERSHIP_CATEGORY_ALREADY_ALLOWED";
        public const string CategoryNotFound = "FAMILY_MEMBERSHIP_CATEGORY_NOT_FOUND";
        public const string CannotChangeAccessLevelWithCategoriesConfigured =
            "FAMILY_MEMBERSHIP_CANNOT_CHANGE_ACCESS_LEVEL_WITH_CATEGORIES_CONFIGURED";
        public const string MinorMustHaveFullAccess =
            "FAMILY_MEMBERSHIP_MINOR_MUST_HAVE_FULL_ACCESS";
        public const string CannotChangeAccessLevelWhileMinor =
            "FAMILY_MEMBERSHIP_CANNOT_CHANGE_ACCESS_LEVEL_WHILE_MINOR";
        public const string InvalidLegalAuthorityForRole =
            "FAMILY_MEMBERSHIP_INVALID_LEGAL_AUTHORITY_FOR_ROLE";
        public const string LegalAuthorityRequiresMinor =
            "FAMILY_MEMBERSHIP_LEGAL_AUTHORITY_REQUIRES_MINOR";
        public const string MinorRequiresLegalAuthority =
            "FAMILY_MEMBERSHIP_MINOR_REQUIRES_LEGAL_AUTHORITY";
    }

    public static class AppointmentConsentGrant
    {
        public const string UnauthorizedCreation =
            "APPOINTMENT_CONSENT_GRANT_UNAUTHORIZED_CREATION";
        public const string PatientMustBeMinor = "APPOINTMENT_CONSENT_GRANT_PATIENT_MUST_BE_MINOR";
        public const string ScopeMismatch = "APPOINTMENT_CONSENT_GRANT_SCOPE_MISMATCH";
        public const string CategoryNotGrantable =
            "APPOINTMENT_CONSENT_GRANT_CATEGORY_NOT_GRANTABLE";
        public const string AppointmentNotGrantable =
            "APPOINTMENT_CONSENT_GRANT_APPOINTMENT_NOT_GRANTABLE";
        public const string AppointmentPatientMismatch =
            "APPOINTMENT_CONSENT_GRANT_APPOINTMENT_PATIENT_MISMATCH";
        public const string RecipientMustBeActive =
            "APPOINTMENT_CONSENT_GRANT_RECIPIENT_MUST_BE_ACTIVE";
        public const string RecipientPatientMismatch =
            "APPOINTMENT_CONSENT_GRANT_RECIPIENT_PATIENT_MISMATCH";
        public const string RecipientCannotBeSelf =
            "APPOINTMENT_CONSENT_GRANT_RECIPIENT_CANNOT_BE_SELF";
        public const string AlreadyExists = "APPOINTMENT_CONSENT_GRANT_ALREADY_EXISTS";
        public const string UnauthorizedRevocation =
            "APPOINTMENT_CONSENT_GRANT_UNAUTHORIZED_REVOCATION";
        public const string AlreadyRevoked = "APPOINTMENT_CONSENT_GRANT_ALREADY_REVOKED";
        public const string AlreadyExpired = "APPOINTMENT_CONSENT_GRANT_ALREADY_EXPIRED";
    }

    public static class MedicalRecordConsentGrant
    {
        public const string UnauthorizedCreation =
            "MEDICAL_RECORD_CONSENT_GRANT_UNAUTHORIZED_CREATION";
        public const string PatientMustBeMinor =
            "MEDICAL_RECORD_CONSENT_GRANT_PATIENT_MUST_BE_MINOR";
        public const string RecordPatientMismatch =
            "MEDICAL_RECORD_CONSENT_GRANT_RECORD_PATIENT_MISMATCH";
        public const string RecordMustHaveProtectedCategory =
            "MEDICAL_RECORD_CONSENT_GRANT_RECORD_MUST_HAVE_PROTECTED_CATEGORY";
        public const string CategoryNotGrantable =
            "MEDICAL_RECORD_CONSENT_GRANT_CATEGORY_NOT_GRANTABLE";
        public const string RecipientMustBeActive =
            "MEDICAL_RECORD_CONSENT_GRANT_RECIPIENT_MUST_BE_ACTIVE";
        public const string RecipientPatientMismatch =
            "MEDICAL_RECORD_CONSENT_GRANT_RECIPIENT_PATIENT_MISMATCH";
        public const string RecipientCannotBeSelf =
            "MEDICAL_RECORD_CONSENT_GRANT_RECIPIENT_CANNOT_BE_SELF";
        public const string AlreadyExists = "MEDICAL_RECORD_CONSENT_GRANT_ALREADY_EXISTS";
        public const string UnauthorizedRevocation =
            "MEDICAL_RECORD_CONSENT_GRANT_UNAUTHORIZED_REVOCATION";
        public const string AlreadyRevoked = "MEDICAL_RECORD_CONSENT_GRANT_ALREADY_REVOKED";
        public const string AlreadyExpired = "MEDICAL_RECORD_CONSENT_GRANT_ALREADY_EXPIRED";
        public const string UnauthorizedAccess = "MEDICAL_RECORD_CONSENT_GRANT_UNAUTHORIZED_ACCESS";
    }
}
