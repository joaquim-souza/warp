using System.Xml.Linq;
using Warp.Core.Model;

namespace Warp.Core.Parsing;

/// <summary>
/// Converte XML arbitrário em árvore canônica, usando <see cref="XDocument"/>
/// (built-in do .NET). Atributos XML viram <see cref="CanonicalNode.Attributes"/>;
/// elementos filhos viram <see cref="CanonicalNode.Children"/>; texto direto
/// do elemento (sem filhos) vira <see cref="CanonicalNode.Value"/>.
/// </summary>
public sealed class XmlParser : ICanonicalParser
{
    public string FormatName => "xml";

    public CanonicalDocument Parse(Stream input)
    {
        var xdoc = XDocument.Load(input);
        if (xdoc.Root is null)
        {
            return new CanonicalDocument(new CanonicalNode("root"), FormatName);
        }

        var root = ConvertElement(xdoc.Root);
        return new CanonicalDocument(root, FormatName);
    }

    private CanonicalNode ConvertElement(XElement element)
    {
        var node = new CanonicalNode(element.Name.LocalName);

        foreach (var attribute in element.Attributes())
        {
            node.Attributes[attribute.Name.LocalName] = attribute.Value;
        }

        var childElements = element.Elements().ToList();
        if (childElements.Count == 0)
        {
            // Nó folha — o texto direto do elemento vira o Value.
            var text = element.Value;
            node.Value = string.IsNullOrEmpty(text) ? null : text;
        }
        else
        {
            foreach (var child in childElements)
            {
                node.AddChild(ConvertElement(child));
            }
        }

        return node;
    }
}