namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;

/// <summary>
/// Defines the common structure for commands that manage clinical form templates.
/// </summary>
public interface IClinicalFormTemplateCommand
{
    string Name { get; }

    string Description { get; }

    /// <summary>
    /// Gets the raw JSON schema draft-07 representation used to validate the form's data entries.
    /// </summary>
    string JsonSchemaDefinition { get; }
}
