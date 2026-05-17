using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Handles login authentication logic, recording successful or failed attempts
/// and enforcing lockout rules.
/// </summary>
public static class UserAuthenticationService
{
    public static bool TryAuthenticate(User user, bool isPasswordValid, DateTime loginTime)
    {
        if (user is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (!isPasswordValid)
        {
            user.RecordFailedLogin(loginTime);
            return false;
        }

        user.RecordLogin(loginTime);
        return true;
    }
}
