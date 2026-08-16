using System.Text;
using Warp.Core.Parsing;

namespace Tests.Parsing;

public sealed class XmlParserTests
{
    [Fact]
    public void Parse_ShouldBuildCanonicalTree()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <PurchaseOrder>
                <OrderID>PO-1001</OrderID>
                <Supplier>
                    <Name>Acme Supplies</Name>
                </Supplier>
                <Buyer>
                    <Name>Contoso Corporation</Name>
                </Buyer>
                <Item>
                    <SKU>KB-001</SKU>
                    <Description>Mechanical Keyboard</Description>
                    <Quantity>10</Quantity>
                    <UnitPrice>99.90</UnitPrice>
                </Item>
            </PurchaseOrder>
            """;

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(xml));

        var document = new XmlParser().Parse(stream);

        Assert.Equal("xml", document.SourceFormat);
        Assert.Equal("PurchaseOrder", document.Root.Name);

        Assert.Equal(
            "PO-1001",
            document.Root.Navigate("OrderID")?.Value);

        Assert.Equal(
            "Acme Supplies",
            document.Root.Navigate("Supplier.Name")?.Value);

        Assert.Equal(
            "Contoso Corporation",
            document.Root.Navigate("Buyer.Name")?.Value);

        Assert.Equal(
            "KB-001",
            document.Root.Navigate("Item.SKU")?.Value);

        Assert.Equal(
            "10",
            document.Root.Navigate("Item.Quantity")?.Value);
    }
}