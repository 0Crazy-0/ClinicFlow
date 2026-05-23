namespace ClinicFlow.Infrastructure.Persistence;

internal static class ColumnNames
{
    internal static class Appointment
    {
        public const string StartTime = "StartTime";
        public const string EndTime = "EndTime";
    }

    internal static class AppointmentTypeDefinition
    {
        public const string AgePolicyMinimumAge = "AgePolicyMinimumAge";
        public const string AgePolicyMaximumAge = "AgePolicyMaximumAge";
        public const string AgePolicyRequiresLegalGuardian = "AgePolicyRequiresLegalGuardian";
    }

    internal static class Doctor
    {
        public const string ConsultationRoomNumber = "ConsultationRoomNumber";
        public const string ConsultationRoomName = "ConsultationRoomName";
        public const string ConsultationRoomFloor = "ConsultationRoomFloor";
    }

    internal static class Patient
    {
        public const string EmergencyContactName = "EmergencyContactName";
        public const string EmergencyContactPhone = "EmergencyContactPhone";
    }

    internal static class Schedule
    {
        public const string StartTime = "StartTime";
        public const string EndTime = "EndTime";
    }
}
