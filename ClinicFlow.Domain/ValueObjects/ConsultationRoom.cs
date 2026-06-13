using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record ConsultationRoom
{
    public const int MinimumNumber = 1;
    public const int MaximumNumber = 35;
    public const int MinimumFloor = 1;
    public const int MaximumFloor = 8;

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
        if (number < MinimumNumber)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        if (number > MaximumNumber)
            throw new DomainValidationException(DomainErrors.Validation.ValueExceedsMaximum);

        if (floor < MinimumFloor)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        if (floor > MaximumFloor)
            throw new DomainValidationException(DomainErrors.Validation.ValueExceedsMaximum);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new ConsultationRoom(number, name.Trim(), floor);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Room {Number} - {Name} (Floor {Floor})";
}
