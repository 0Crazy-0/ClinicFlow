using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Cancellation;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for enforcing invariant rules when cancelling an appointment.
/// </summary>
public static class AppointmentCancellationService
{
    public static void CancelByPatient(Appointment appointment, PatientCancellationArgs args)
    {
        if (appointment.PatientId != args.AppointmentPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (args.InitiatorPatient is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.AppointmentPatient.UserId != args.InitiatorPatient.UserId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        if (args.Category is AppointmentCategory.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (args.Category is AppointmentCategory.Emergency)
            ValidateEmergencyCancellation(args.AppointmentPatient);

        appointment.Cancel(args.InitiatorPatient.UserId, args.Reason, args.Specialty);
    }

    public static void CancelByDoctor(Appointment appointment, DoctorCancellationArgs args)
    {
        if (args.InitiatorDoctor is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(args.InitiatorDoctor.UserId, args.Reason, args.Specialty, true);
    }

    public static void CancelByStaff(Appointment appointment, StaffCancellationArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        if (args.Role != UserRole.Admin && args.Role != UserRole.Receptionist)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        appointment.Cancel(args.InitiatorUserId, args.Reason, args.Specialty, true);
    }

    public static void ValidateEmergencyCancellation(Patient patient)
    {
        if (patient.RelationshipToUser is PatientRelationship.Self)
            return;

        if (patient.RelationshipToUser is PatientRelationship.Child && patient.GetAge() < 18)
            return;

        throw new AppointmentCancellationUnauthorizedException(
            DomainErrors.Appointment.CannotCancel
        );
    }
}
