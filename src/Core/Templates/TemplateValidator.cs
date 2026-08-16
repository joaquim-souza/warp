namespace Warp.Core.Templates;

/// <summary>
/// Valida a estrutura de um TemplateDefinition antes que ele seja usado
/// pelo Transformer.
///
/// A validação aqui é estrutural. Regras dependentes do formato de entrada
/// ou saída podem ser adicionadas posteriormente.
/// </summary>
public sealed class TemplateValidator
{
    public TemplateValidationResult Validate(TemplateDefinition template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var result = new TemplateValidationResult();

        if (string.IsNullOrWhiteSpace(template.Id))
        {
            result.AddError("Id", "Template deve possuir um Id.");
        }

        if (template.Version <= 0)
        {
            result.AddError(
                "Version",
                "Versão do template deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(template.SourceFormat))
        {
            result.AddError(
                "SourceFormat",
                "Template deve possuir um SourceFormat.");
        }

        if (string.IsNullOrWhiteSpace(template.TargetFormat))
        {
            result.AddError(
                "TargetFormat",
                "Template deve possuir um TargetFormat.");
        }

        if (template.Mappings.Count == 0)
        {
            result.AddError(
                "Mappings",
                "Template deve possuir pelo menos um mapping.");
        }

        for (int i = 0; i < template.Mappings.Count; i++)
        {
            var mapping = template.Mappings[i];

            if (string.IsNullOrWhiteSpace(mapping.SourcePath))
            {
                result.AddError(
                    $"Mappings[{i}].SourcePath",
                    "SourcePath não pode ser vazio.");
            }

            if (string.IsNullOrWhiteSpace(mapping.TargetPath))
            {
                result.AddError(
                    $"Mappings[{i}].TargetPath",
                    "TargetPath não pode ser vazio.");
            }
        }

        return result;
    }
}

public sealed class TemplateValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<TemplateValidationError> Errors { get; } = new();

    public void AddError(string path, string message) =>
        Errors.Add(new TemplateValidationError(path, message));
}

public sealed record TemplateValidationError(
    string Path,
    string Message);