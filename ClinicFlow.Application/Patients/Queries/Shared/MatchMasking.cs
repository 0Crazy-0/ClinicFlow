namespace ClinicFlow.Application.Patients.Queries.Shared;

public static class MatchMasking
{
    public static string? MaskEmail(string? email)
    {
        if (email is null)
            return null;

        var separatorIndex = email.IndexOf('@');
        if (separatorIndex <= 0)
            return "***";

        var local = email[..separatorIndex];
        var domain = email[separatorIndex..];

        var visible = Math.Min(3, local.Length / 2);

        return $"{local[..visible]}***{domain}";
    }

    public static string? MaskPhone(string? phone)
    {
        if (phone is null)
            return null;

        var digits = new string(phone.Where(char.IsAsciiDigit).ToArray());
        return digits.Length > 4 ? $"****{digits[^4..]}" : "****";
    }
}
