using System.Text;
using Warp.Core.Model;
using Warp.Core.Parsing;
using Warp.Core.Serialization;

namespace Tests.Serialization;

public sealed class XmlRoundTripTests
{
    [Fact]
    public void Xml_ShouldSurviveParseSerializeParse()
    {
        var root = new CanonicalNode("cXML");

        var header = root.AddChild("Header");

        header.AddChild("OrderID", "PO-1001");

        var original =
            new CanonicalDocument(root, "xml");

        using var buffer = new MemoryStream();

        new XmlCanonicalSerializer()
            .Serialize(original, buffer);

        buffer.Position = 0;

        var reparsed =
            new XmlParser().Parse(buffer);

        Assert.Equal(
            "PO-1001",
            reparsed.Root.Navigate("Header.OrderID")?.Value);
    }
}