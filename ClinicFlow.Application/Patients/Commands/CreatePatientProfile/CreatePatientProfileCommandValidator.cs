using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidator(TimeProvider timeProvider)
    : CreatePatientCommandValidatorBase<CreatePatientProfileCommand>(timeProvider) { }
