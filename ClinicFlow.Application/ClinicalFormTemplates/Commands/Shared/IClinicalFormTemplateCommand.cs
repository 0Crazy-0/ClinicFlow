namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;

public interface IClinicalFormTemplateCommand
{
    string Name { get; }
    string Description { get; }
    string JsonSchemaDefinition { get; }
}
