using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Defines a dynamic clinical form template that specifies the required data schema for an appointment.
/// </summary>
public class ClinicalFormTemplate : BaseEntity
{
    /// <summary>
    /// Immutable natural key that external systems and the frontend use to
    /// reference this template. Changing it would break historical records.
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// JSON string defining the schema, fields, and types required for this template.
    /// This acts as the metadata for the frontend to render the form and the backend to optionally validate it.
    /// </summary>
    public string JsonSchemaDefinition { get; private set; } = "{}";

    // EF Core constructor
    private ClinicalFormTemplate() { }

    private ClinicalFormTemplate(
        string code,
        string name,
        string description,
        string jsonSchemaDefinition
    )
    {
        Code = code;
        Name = name;
        Description = description;
        JsonSchemaDefinition = jsonSchemaDefinition;
    }

    /// <summary>
    /// Creates a new clinical form template.
    /// </summary>
    public static ClinicalFormTemplate Create(
        string code,
        string name,
        string description,
        string jsonSchemaDefinition
    )
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        string schemaToSave = string.IsNullOrWhiteSpace(jsonSchemaDefinition)
            ? "{}"
            : jsonSchemaDefinition;

        return new ClinicalFormTemplate(code, name, description, schemaToSave);
    }

    /// <summary>
    /// Updates descriptive details. Code is immutable: it serves as the business identity.
    /// </summary>
    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        Name = name;
        Description = description;
    }

    /// <summary>
    /// Replaces the current JSON schema definition. Defaults to an empty object if null or whitespace.
    /// </summary>
    public void UpdateSchema(string jsonSchemaDefinition) =>
        JsonSchemaDefinition = string.IsNullOrWhiteSpace(jsonSchemaDefinition)
            ? "{}"
            : jsonSchemaDefinition;
}
