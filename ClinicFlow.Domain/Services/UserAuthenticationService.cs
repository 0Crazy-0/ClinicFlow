using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Handles login authentication logic, recording successful or failed attempts
/// and enforcing lockout rules.
/// </summary>
public static class UserAuthenticationService
{
    public static bool TryAuthenticate(User user, bool isPasswordValid, DateTime loginTime)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin(loginTime);
            return false;
        }

        user.RecordLogin(loginTime);
        return true;
    }
}
