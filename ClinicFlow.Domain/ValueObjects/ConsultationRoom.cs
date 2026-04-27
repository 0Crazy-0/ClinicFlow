using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record ConsultationRoom
{
    public int Number { get; }

    public string Name { get; }

    public int Floor { get; }

    private ConsultationRoom(int number, string name, int floor)
    {
        Number = number;
        Name = name;
        Floor = floor;
    }

    public static ConsultationRoom Create(int number, string name, int floor)
    {
        if (number <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (floor <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        return new ConsultationRoom(number, name.Trim(), floor);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Room {Number} - {Name} (Floor {Floor})";
}
