using System.Reflection;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Application.Tests.Shared;

public static class EntityTestExtensions
{
    public static void SetId(this BaseEntity entity, Guid id)
    {
        var property = typeof(BaseEntity).GetProperty(
            nameof(BaseEntity.Id),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        property?.SetValue(entity, id);
    }
}
