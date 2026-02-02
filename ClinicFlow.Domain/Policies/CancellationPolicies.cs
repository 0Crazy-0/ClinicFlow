namespace ClinicFlow.Domain.Policies;

public static class CancellationPolicies
{
    public const int FirstConsultationHours = 24;
    public const int FollowUpHours = 12;
    public const int EmergencyHours = 2;
    public const int CheckupHours = 24;
    public const int ProcedureHours = 48;
}
