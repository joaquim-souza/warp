using System.Xml.Linq;
using Warp.Core.Model;

namespace Warp.Core.Serialization;

/// <summary>
/// Serializa a árvore canônica de volta para XML genérico. Se o documento
/// veio de um template com RecordsPath (múltiplos registros), envolve tudo
/// numa raiz "records"; senão, serializa o registro único diretamente como
/// elemento raiz.
/// </summary>
public sealed class XmlCanonicalSerializer : ICanonicalSerializer
{
    public string FormatName => "xml";

    public void Serialize(CanonicalDocument document, Stream output)
    {
        var xElement = ConvertNode(document.Root);
        var xdoc = new XDocument(new XDeclaration("1.0", "UTF-8", null), xElement);
        xdoc.Save(output);
    }

    private XElement ConvertNode(CanonicalNode node)
    {
        var element = new XElement(SanitizeElementName(node.Name));

        foreach (var (key, value) in node.Attributes)
        {
            element.SetAttributeValue(key, value);
        }

        if (node.Children.Count == 0)
        {
            if (node.Value is not null)
            {
                element.Value = node.Value;
            }
        }
        else
        {
            foreach (var child in node.Children)
            {
                element.Add(ConvertNode(child));
            }
        }

        return element;
    }

    /// <summary>XML não aceita nome de elemento começando com dígito ou contendo espaço — sanitiza o mínimo necessário.</summary>
    private static string SanitizeElementName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "field";
        var sanitized = new string(name.Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_').ToArray());
        return char.IsDigit(sanitized[0]) ? "_" + sanitized : sanitized;
    }
}