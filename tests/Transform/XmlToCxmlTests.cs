using System.Text;
using Warp.Core.Parsing;
using Warp.Core.Templates;
using Warp.Core.Transform;

namespace Tests.Transform;

public sealed class XmlToCxmlTests
{
    [Fact]
    public void Transform_ShouldConvertPurchaseOrderToCxmlStructure()
    {
        const string xml = """
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

        using var input = new MemoryStream(
            Encoding.UTF8.GetBytes(xml));

        var source = new XmlParser().Parse(input);

        var template = new TemplateDefinition
        {
            Id = "xml-to-cxml",
            Version = 1,
            SourceFormat = "xml",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "OrderID",
                    TargetPath = "cXML.Header.PunchOutSetupRequest.OrderID",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Supplier.Name",
                    TargetPath = "cXML.Header.Supplier.Name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Buyer.Name",
                    TargetPath = "cXML.Header.Buyer.Name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Item.SKU",
                    TargetPath = "cXML.Request.OrderRequest.ItemOut.SupplierPartID",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Item.Description",
                    TargetPath = "cXML.Request.OrderRequest.ItemOut.Description",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Item.Quantity",
                    TargetPath = "cXML.Request.OrderRequest.ItemOut.Quantity",
                    Required = true,
                    Transform = TransformType.ToNumber
                },
                new FieldMapping
                {
                    SourcePath = "Item.UnitPrice",
                    TargetPath = "cXML.Request.OrderRequest.ItemOut.UnitPrice",
                    Required = true,
                    Transform = TransformType.ToNumber
                }
            ]
        };

        var (output, validation) =
            new Transformer().Transform(source, template);

        Assert.True(validation.IsValid);

        Assert.Equal(
            "PO-1001",
            output.Root.Navigate(
                "cXML.Header.PunchOutSetupRequest.OrderID")?.Value);

        Assert.Equal(
            "Acme Supplies",
            output.Root.Navigate(
                "cXML.Header.Supplier.Name")?.Value);

        Assert.Equal(
            "Contoso Corporation",
            output.Root.Navigate(
                "cXML.Header.Buyer.Name")?.Value);

        Assert.Equal(
            "KB-001",
            output.Root.Navigate(
                "cXML.Request.OrderRequest.ItemOut.SupplierPartID")?.Value);

        Assert.Equal(
            "10",
            output.Root.Navigate(
                "cXML.Request.OrderRequest.ItemOut.Quantity")?.Value);

        Assert.Equal(
            "99.9",
            output.Root.Navigate(
                "cXML.Request.OrderRequest.ItemOut.UnitPrice")?.Value);
    }
}