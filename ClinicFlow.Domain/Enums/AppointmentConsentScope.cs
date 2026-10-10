namespace ClinicFlow.Domain.Enums;

public enum AppointmentConsentScope
{
    /// <summary>
    /// Authorizes every appointment of one protected care category.
    /// </summary>
    Category,

    /// <summary>
    /// Authorizes a single appointment by id.
    /// </summary>
    SpecificAppointment,
}
