using System.Globalization;
using Warp.Core.Model;
using Warp.Core.Templates;

namespace Warp.Core.Transform;

/// <summary>
/// Aplica um <see cref="TemplateDefinition"/> a um <see cref="CanonicalDocument"/>,
/// produzindo um novo documento canônico na forma que o serializer de
/// destino espera.
/// <para/>
/// <b>Determinismo é a regra central do WARP</b> ("mesmo input + mesmo
/// template = mesmo output") — por isso essa classe não tem NENHUM estado
/// mutável entre chamadas, não lê relógio, não gera valor aleatório. Se um
/// template precisar de timestamp/ID (ex: envelope cXML), isso é
/// responsabilidade do chamador injetar explicitamente via contexto — ver
/// <see cref="Warp.Core.Time.IClock"/> e <see cref="Warp.Core.Engine.WarpEngine"/>.
/// </summary>
public sealed class Transformer
{
    public (CanonicalDocument Output, ValidationResult Validation) Transform(
        CanonicalDocument source, TemplateDefinition template)
    {
        var validation = new ValidationResult();

        if (string.IsNullOrEmpty(template.RecordsPath))
        {
            var singleRecord = TransformRecord(source.Root, template, recordIndex: null, validation);
            return (new CanonicalDocument(singleRecord, template.TargetFormat), validation);
        }

        var outputRoot = new CanonicalNode("records");
        var sourceRecords = source.Root.ChildrenNamed(template.RecordsPath).ToList();

        for (int i = 0; i < sourceRecords.Count; i++)
        {
            var transformedRecord = TransformRecord(sourceRecords[i], template, recordIndex: i, validation);
            outputRoot.AddChild(transformedRecord);
        }

        return (new CanonicalDocument(outputRoot, template.TargetFormat), validation);
    }

    private CanonicalNode TransformRecord(
        CanonicalNode sourceRecord, TemplateDefinition template, int? recordIndex, ValidationResult validation)
    {
        var outputRecord = new CanonicalNode("record");

        foreach (var mapping in template.Mappings)
        {
            string recordLabel = recordIndex is int idx ? $"registro[{idx}]" : "documento";
            string errorPath = $"{recordLabel}.{mapping.SourcePath}";

            var sourceNode = sourceRecord.Navigate(mapping.SourcePath);
            string? rawValue = sourceNode?.Value;

            if (rawValue is null)
            {
                if (mapping.DefaultValue is not null)
                {
                    rawValue = mapping.DefaultValue;
                }
                else if (mapping.Required)
                {
                    validation.AddError(errorPath,
                        $"Campo obrigatório ausente: '{mapping.SourcePath}' não encontrado e sem DefaultValue");
                    continue;
                }
                else
                {
                    continue; // opcional, ausente, sem default — simplesmente não entra na saída
                }
            }

            string? transformedValue = ApplyTransform(rawValue, mapping.Transform, errorPath, validation);
            if (transformedValue is null)
            {
                continue; // erro de transform já registrado em validation
            }

            SetPath(outputRecord, mapping.TargetPath, transformedValue);
        }

        return outputRecord;
    }

    /// <summary>
    /// Cria (ou reaproveita) os nós intermediários de um TargetPath com ponto
    /// (ex: "item.unitPrice" → nó "item" contendo filho "unitPrice"), e
    /// define o valor no nó folha final.
    /// </summary>
    private static void SetPath(CanonicalNode root, string dottedPath, string value)
    {
        var segments = dottedPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var current = root;
        for (int i = 0; i < segments.Length - 1; i++)
        {
            var existing = current.Child(segments[i]);
            current = existing ?? current.AddChild(segments[i]);
        }
        current.AddChild(segments[^1], value);
    }

    private static string? ApplyTransform(
        string value, TransformType transform, string errorPath, ValidationResult validation)
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
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
                {
                    return number.ToString(CultureInfo.InvariantCulture);
                }
                validation.AddError(errorPath, $"Valor '{value}' não pôde ser convertido para número");
                return null;
            case TransformType.ToDateIso8601:
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                validation.AddError(errorPath, $"Valor '{value}' não pôde ser convertido para data ISO 8601");
                return null;
            default:
                return value;
        }
    }
}