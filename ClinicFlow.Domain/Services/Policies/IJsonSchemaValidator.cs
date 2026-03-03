namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Domain interface for validating dynamic JSON clinic payloads against metadata schemas.
/// The implementation of this interface should reside in the Infrastructure layer 
/// to keep the Domain pure from serialization or framework logic.
/// </summary>
public interface IJsonSchemaValidator
{
    /// <summary>
    /// Validates if a JSON payload complies with the requirements defined in a JSON schema.
    /// </summary>
    /// <param name="schemaDefinition">The JSON string representing the schema (e.g., from ClinicalFormTemplate).</param>
    /// <param name="jsonDataPayload">The JSON string representing the provided answers (e.g., from DynamicClinicalDetail).</param>
    /// <param name="errorMessage">The error message returned if validation fails.</param>
    /// <returns>True if the payload is valid; otherwise false.</returns>
    bool ValidateSchema(string schemaDefinition, string jsonDataPayload, out string? errorMessage);
}
