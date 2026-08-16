using System.Globalization;
using Warp.Core.Model;
using Warp.Core.Templates;

namespace Warp.Core.Transform;

/// <summary>
/// Aplica um TemplateDefinition a um CanonicalDocument,
/// produzindo um novo documento canônico na forma que o serializer
/// de destino espera.
///
/// O Transformer é determinístico:
/// não possui estado mutável entre chamadas, não lê relógio
/// e não gera valores aleatórios.
/// </summary>
public sealed class Transformer
{
    public (CanonicalDocument Output, ValidationResult Validation) Transform(
        CanonicalDocument source,
        TemplateDefinition template)
    {
        var validation = new ValidationResult();

        if (string.IsNullOrEmpty(template.RecordsPath))
        {
            var outputRootName =
                string.IsNullOrWhiteSpace(template.OutputRoot)
                    ? "record"
                    : template.OutputRoot;

            var singleRecord =
                TransformRecord(
                    source.Root,
                    template,
                    recordIndex: null,
                    validation,
                    outputRootName);

            return (
                new CanonicalDocument(
                    singleRecord,
                    template.TargetFormat),
                validation);
        }

        var outputRootNameForRecords =
            string.IsNullOrWhiteSpace(template.OutputRoot)
                ? "records"
                : template.OutputRoot;

        var outputRoot =
            new CanonicalNode(
                outputRootNameForRecords);

        var sourceRecords =
            source.Root
                .ChildrenNamed(template.RecordsPath)
                .ToList();

        for (int i = 0; i < sourceRecords.Count; i++)
        {
            var transformedRecord =
                TransformRecord(
                    sourceRecords[i],
                    template,
                    recordIndex: i,
                    validation,
                    "record");

            outputRoot.AddChild(
                transformedRecord);
        }

        return (
            new CanonicalDocument(
                outputRoot,
                template.TargetFormat),
            validation);
    }

    private CanonicalNode TransformRecord(
        CanonicalNode sourceRecord,
        TemplateDefinition template,
        int? recordIndex,
        ValidationResult validation,
        string outputName)
    {
        var outputRecord =
            new CanonicalNode(outputName);

        foreach (var mapping in template.Mappings)
        {
            TransformMapping(
                sourceRecord,
                outputRecord,
                mapping,
                recordIndex,
                validation);
        }

        foreach (var collection in template.Collections)
        {
            TransformCollection(
                sourceRecord,
                outputRecord,
                collection,
                recordIndex,
                validation);
        }

        return outputRecord;
    }

    private static void TransformMapping(
        CanonicalNode sourceRecord,
        CanonicalNode outputRecord,
        FieldMapping mapping,
        int? recordIndex,
        ValidationResult validation)
    {
        string recordLabel =
            recordIndex is int idx
                ? $"registro[{idx}]"
                : "documento";

        string errorPath =
            $"{recordLabel}.{mapping.SourcePath}";

        var sourceNode =
            sourceRecord.Navigate(
                mapping.SourcePath);

        string? rawValue =
            sourceNode?.Value;

        if (rawValue is null)
        {
            if (mapping.DefaultValue is not null)
            {
                rawValue =
                    mapping.DefaultValue;
            }
            else if (mapping.Required)
            {
                validation.AddError(
                    errorPath,
                    $"Campo obrigatório ausente: '{mapping.SourcePath}' não encontrado e sem DefaultValue");

                return;
            }
            else
            {
                return;
            }
        }

        var transformedValue =
            ApplyTransform(
                rawValue,
                mapping.Transform,
                errorPath,
                validation);

        if (transformedValue is null)
        {
            return;
        }

        SetPath(
            outputRecord,
            mapping.TargetPath,
            transformedValue);
    }

    private static void TransformCollection(
        CanonicalNode sourceRecord,
        CanonicalNode outputRecord,
        CollectionMapping collection,
        int? recordIndex,
        ValidationResult validation)
    {
        var sourceCollection =
            sourceRecord.Navigate(
                collection.SourcePath);

        if (sourceCollection is null)
        {
            if (collection.Required)
            {
                string recordLabel =
                    recordIndex is int idx
                        ? $"registro[{idx}]"
                        : "documento";

                validation.AddError(
                    $"{recordLabel}.{collection.SourcePath}",
                    $"Coleção obrigatória ausente: '{collection.SourcePath}'.");
            }

            return;
        }

        var targetSegments =
            collection.TargetPath.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries);

        if (targetSegments.Length == 0)
        {
            return;
        }

        var parent =
            GetOrCreateParentPath(
                outputRecord,
                targetSegments);

        var itemName =
            targetSegments[^1];

        foreach (var sourceItem in sourceCollection.Children)
        {
            var targetItem =
                parent.AddChild(
                    itemName);

            foreach (var mapping in collection.Mappings)
            {
                TransformMapping(
                    sourceItem,
                    targetItem,
                    mapping,
                    recordIndex,
                    validation);
            }
        }
    }

    private static void SetPath(
        CanonicalNode root,
        string dottedPath,
        string value)
    {
        var segments =
            dottedPath.Split(
                '.',
                StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return;
        }

        var current =
            root;

        for (int i = 0;
             i < segments.Length - 1;
             i++)
        {
            var existing =
                current.Child(
                    segments[i]);

            current =
                existing ??
                current.AddChild(
                    segments[i]);
        }

        current.AddChild(
            segments[^1],
            value);
    }

    private static CanonicalNode GetOrCreateParentPath(
        CanonicalNode root,
        IReadOnlyList<string> segments)
    {
        var current =
            root;

        for (int i = 0;
             i < segments.Count - 1;
             i++)
        {
            var existing =
                current.Child(
                    segments[i]);

            current =
                existing ??
                current.AddChild(
                    segments[i]);
        }

        return current;
    }

    private static string? ApplyTransform(
        string value,
        TransformType transform,
        string errorPath,
        ValidationResult validation)
    {
        switch (transform)
        {
            case TransformType.None:
                return value;

            case TransformType.Trim:
                return value.Trim();

            case TransformType.Upper:
                return value.ToUpperInvariant();

            case TransformType.Lower:
                return value.ToLowerInvariant();

            case TransformType.ToNumber:
                if (decimal.TryParse(
                    value,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var number))
                {
                    return number.ToString(
                        CultureInfo.InvariantCulture);
                }

                validation.AddError(
                    errorPath,
                    $"Valor '{value}' não pôde ser convertido para número");

                return null;

            case TransformType.ToDateIso8601:
                if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                {
                    return date.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);
                }

                validation.AddError(
                    errorPath,
                    $"Valor '{value}' não pôde ser convertido para data ISO 8601");

                return null;

            default:
                return value;
        }
    }
}