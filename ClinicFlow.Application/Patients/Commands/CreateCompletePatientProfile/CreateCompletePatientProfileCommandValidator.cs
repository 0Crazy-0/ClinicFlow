using ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;

namespace ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandValidator(TimeProvider timeProvider)
    : CompletePatientCommandValidatorBase<CreateCompletePatientProfileCommand>(timeProvider) { }
