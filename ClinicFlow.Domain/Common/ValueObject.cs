namespace ClinicFlow.Domain.Common;

public abstract class ValueObject
{
    protected static bool EqualOperator(ValueObject left, ValueObject right) => left?.Equals(right) ?? right is null;

    protected static bool NotEqualOperator(ValueObject left, ValueObject right) => !EqualOperator(left, right);

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component?.GetHashCode() ?? 0);
        }

        return hash.ToHashCode();
    }
}
