using System.Reflection;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainEventsTests
{
    [Fact]
    public void BaseEntity_DomainEvents_ShouldBe_ReadOnlyCollectionOfIDomainEvent()
    {
        var property = typeof(BaseEntity).GetProperty(
            nameof(BaseEntity.DomainEvents),
            BindingFlags.Public | BindingFlags.Instance
        );

        property.Should().NotBeNull();

        property.PropertyType.Should().Be(typeof(IReadOnlyCollection<IDomainEvent>));
        property.PropertyType.GetGenericArguments().Single().Should().Be(typeof(IDomainEvent));
    }

    [Fact]
    public void DomainEntities_ShouldNotHide_DomainEventsProperty()
    {
        var entityTypes = typeof(BaseEntity)
            .Assembly.GetTypes()
            .Where(type =>
                type.IsClass
                && type != typeof(BaseEntity)
                && typeof(BaseEntity).IsAssignableFrom(type)
            )
            .ToList();

        entityTypes.Should().NotBeEmpty();

        var violations = entityTypes
            .Where(type =>
                type.GetProperty(
                    nameof(BaseEntity.DomainEvents),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                )
                    is not null
                || type.GetProperty(
                    nameof(BaseEntity.DomainEvents),
                    BindingFlags.Public | BindingFlags.Instance
                )
                    is not { DeclaringType: var declaringType, PropertyType: var propertyType }
                || declaringType != typeof(BaseEntity)
                || propertyType != typeof(IReadOnlyCollection<IDomainEvent>)
            )
            .Select(type => type.FullName)
            .ToList();

        violations
            .Should()
            .BeEmpty(
                "every domain entity must inherit DomainEvents as IReadOnlyCollection<IDomainEvent> "
                    + "from BaseEntity without hiding it, otherwise EF Core could discover it as a "
                    + "navigation property once the event type becomes mappable"
            );
    }
}
