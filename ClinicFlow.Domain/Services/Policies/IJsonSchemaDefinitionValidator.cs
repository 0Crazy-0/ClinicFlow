namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Validates whether a JSON string constitutes a structurally valid schema definition
/// suitable for use as a clinical form template.
/// Implementations reside in the Infrastructure layer.
/// </summary>
public interface IJsonSchemaDefinitionValidator
{
    /// <summary>
    /// Determines whether the given JSON string is a valid schema definition.
    /// </summary>
    /// <param name="schemaDefinition">The JSON string representing the schema to validate.</param>
    /// <param name="errorMessage">The error message returned if validation fails; null if valid.</param>
    /// <returns>True if the schema is structurally valid; otherwise false.</returns>
    bool IsValidSchema(string schemaDefinition, out string? errorMessage);
}
