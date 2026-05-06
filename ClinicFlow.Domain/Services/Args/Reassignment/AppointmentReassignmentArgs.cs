using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Reassignment;

public sealed record AppointmentReassignmentArgs
{
    public Guid NewDoctorId { get; init; }
    public DateTime NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
}
