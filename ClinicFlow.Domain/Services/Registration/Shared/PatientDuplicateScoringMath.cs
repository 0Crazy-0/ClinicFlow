using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Common.ThirdParty;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Registration.Shared;

internal static class PatientDuplicateScoringMath
{
    public const int AddressWeight = 5;

    public static double ComputeAddressWeight(Address formAddress, Address candidateAddress)
    {
        if (
            formAddress.State != candidateAddress.State
            || formAddress.PostalCode != candidateAddress.PostalCode
        )
            return 0;

        return AddressWeight
            * Similarity(
                StringNormalizer.Canonicalize(formAddress.Line),
                StringNormalizer.Canonicalize(candidateAddress.Line)
            );
    }

    public static double Similarity(string left, string right)
    {
        if (left.Length == 0 && right.Length == 0)
            return 1.0;

        var distance = Levenshtein.Distance(left, right);

        return 1.0 - distance / (double)Math.Max(left.Length, right.Length);
    }
}
