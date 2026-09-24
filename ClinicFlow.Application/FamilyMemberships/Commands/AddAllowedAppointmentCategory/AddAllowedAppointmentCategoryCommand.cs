using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.AddAllowedAppointmentCategory;

public sealed record AddAllowedAppointmentCategoryCommand(
    Guid RequesterUserId,
    Guid TargetUserId,
    Guid PatientId,
    AppointmentCategory Category
) : IRequest;
