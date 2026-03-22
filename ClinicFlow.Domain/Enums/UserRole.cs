namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Defines the roles a user can hold within the clinic system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// A patient who can book and manage their own appointments.
    /// </summary>
    Patient = 1,

    /// <summary>
    /// A physician who provides medical services.
    /// </summary>
    Doctor = 2,

    /// <summary>
    /// Front-desk staff responsible for scheduling and administrative tasks.
    /// </summary>
    Receptionist = 3,

    /// <summary>
    /// System administrator with full access to all operations.
    /// </summary>
    Admin = 4,
}
