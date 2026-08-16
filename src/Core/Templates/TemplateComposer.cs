namespace Warp.Core.Templates;

/// <summary>
/// Compõe templates derivados a partir de um template base.
///
/// O template filho sobrescreve propriedades escalares e pode
/// adicionar mappings/collections ao template herdado.
/// </summary>
public sealed class TemplateComposer
{
    public TemplateDefinition Compose(
        TemplateDefinition baseTemplate,
        TemplateDefinition childTemplate)
    {
        if (baseTemplate is null)
            throw new ArgumentNullException(nameof(baseTemplate));

        if (childTemplate is null)
            throw new ArgumentNullException(nameof(childTemplate));

        var result = new TemplateDefinition
        {
            Id = childTemplate.Id,
            Version = childTemplate.Version,

            SourceFormat =
                string.IsNullOrWhiteSpace(childTemplate.SourceFormat)
                    ? baseTemplate.SourceFormat
                    : childTemplate.SourceFormat,

            TargetFormat =
                string.IsNullOrWhiteSpace(childTemplate.TargetFormat)
                    ? baseTemplate.TargetFormat
                    : childTemplate.TargetFormat,

            RecordsPath =
                childTemplate.RecordsPath ??
                baseTemplate.RecordsPath,

            OutputRoot =
                childTemplate.OutputRoot ??
                baseTemplate.OutputRoot,

            Extends = childTemplate.Extends,

            Mappings =
            [
                ..baseTemplate.Mappings,
                ..childTemplate.Mappings
            ],

            Collections =
            [
                ..baseTemplate.Collections,
                ..childTemplate.Collections
            ]
        };

        return result;
    }
}