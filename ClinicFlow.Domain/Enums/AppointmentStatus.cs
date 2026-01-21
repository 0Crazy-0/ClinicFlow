namespace ClinicFlow.Domain.Enums
{
    public enum AppointmentStatus
    {
        Scheduled = 1,
        Confirmed = 2,
        InProgress = 3,
        completed = 4,
        Cancelled = 5,
        NoShow = 6,
        LateCancellation = 7
    }
}