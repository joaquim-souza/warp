using System.Text.Json;
using Warp.Core.Model;

namespace Warp.Core.Parsing;

/// <summary>
/// Converte JSON arbitrário em árvore canônica. Regras de conversão:
/// <list type="bullet">
/// <item>objeto {a: 1, b: 2} → nó com um filho por propriedade</item>
/// <item>array [1, 2, 3] → filhos repetidos todos chamados "item"</item>
/// <item>primitivo → nó folha com Value = representação em string</item>
/// </list>
/// Usa <see cref="System.Text.Json"/> (biblioteca padrão do .NET) — sem
/// dependência de terceiro pra JSON.
/// </summary>
public sealed class JsonParser : ICanonicalParser
{
    public string FormatName => "json";

    public CanonicalDocument Parse(Stream input)
    {
        using var doc = JsonDocument.Parse(input);
        var root = ConvertElement("root", doc.RootElement);
        return new CanonicalDocument(root, FormatName);
    }

    private CanonicalNode ConvertElement(string name, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var objNode = new CanonicalNode(name);
                foreach (var property in element.EnumerateObject())
                {
                    objNode.AddChild(ConvertElement(property.Name, property.Value));
                }
                return objNode;

            case JsonValueKind.Array:
                // O nó do array em si vira um container; cada elemento vira
                // um filho "item" — quem consome (Transformer) itera com
                // ChildrenNamed("item"), igual itera "row" para CSV.
                var arrayNode = new CanonicalNode(name);
                foreach (var item in element.EnumerateArray())
                {
                    arrayNode.AddChild(ConvertElement("item", item));
                }
                return arrayNode;

            case JsonValueKind.String:
                return new CanonicalNode(name, element.GetString());

            case JsonValueKind.Number:
                return new CanonicalNode(name, element.GetRawText());

            case JsonValueKind.True:
            case JsonValueKind.False:
                return new CanonicalNode(name, element.GetBoolean().ToString());

            case JsonValueKind.Null:
                return new CanonicalNode(name, null);

            default:
                return new CanonicalNode(name, element.GetRawText());
        }
    }
}