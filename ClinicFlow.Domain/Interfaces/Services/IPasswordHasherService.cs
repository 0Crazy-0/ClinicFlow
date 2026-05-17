namespace ClinicFlow.Domain.Interfaces.Services;

/// <summary>
/// Provides password hashing and verification capabilities.
/// </summary>
public interface IPasswordHasherService
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hashedPassword);
}
