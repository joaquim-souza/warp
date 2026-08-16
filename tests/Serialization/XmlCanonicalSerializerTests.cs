using System.Text;
using System.Xml.Linq;
using Warp.Core.Model;
using Warp.Core.Serialization;

namespace Tests.Serialization;

public sealed class XmlCanonicalSerializerTests
{
    [Fact]
    public void Serialize_ShouldProduceValidXml()
    {
        var root = new CanonicalNode("cXML");

        var header = root.AddChild("Header");

        var request = header.AddChild("Request");

        request.AddChild("OrderID", "PO-1001");

        var document =
            new CanonicalDocument(root, "xml");

        using var output = new MemoryStream();

        new XmlCanonicalSerializer()
            .Serialize(document, output);

        output.Position = 0;

        var xml = XDocument.Load(output);

        Assert.Equal(
            "cXML",
            xml.Root?.Name.LocalName);

        Assert.Equal(
            "PO-1001",
            xml.Root?
                .Element("Header")?
                .Element("Request")?
                .Element("OrderID")?
                .Value);
    }
}