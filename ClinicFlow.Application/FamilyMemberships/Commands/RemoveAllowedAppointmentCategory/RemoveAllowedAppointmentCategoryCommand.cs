using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.FamilyMemberships.Commands.RemoveAllowedAppointmentCategory;

public sealed record RemoveAllowedAppointmentCategoryCommand(
    Guid RequesterUserId,
    Guid TargetUserId,
    Guid PatientId,
    AppointmentCategory Category
) : IRequest;
