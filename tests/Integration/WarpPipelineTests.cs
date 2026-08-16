using System.Text;
using Warp.Core.Engine;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;
using Warp.Excel;

namespace Tests.Integration;

public sealed class WarpPipelineTests
{
    private static WarpEngine CreateEngine()
    {
        var parsers = new ParserRegistry(
        [
            new CsvParser(),
            new JsonParser(),
            new XmlParser()
        ]);

        var serializers = new SerializerRegistry(
        [
            new XmlCanonicalSerializer(),
            new ExcelSerializer()
        ]);

        return new WarpEngine(
            parsers,
            serializers,
            new Transformer());
    }

    [Fact]
    public void CsvToXlsx_ShouldExecuteCompletePipeline()
    {
        const string csv =
            """
            sku,name,price,quantity
            001,Keyboard,99.90,2
            002,Mouse,49.90,5
            """;

        var template = new TemplateDefinition
        {
            Id = "csv-to-xlsx",
            Version = 1,
            SourceFormat = "csv",
            TargetFormat = "xlsx",
            RecordsPath = "row",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "sku",
                    TargetPath = "sku",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "name",
                    TargetPath = "name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "price",
                    TargetPath = "price",
                    Transform = TransformType.ToNumber
                },
                new FieldMapping
                {
                    SourcePath = "quantity",
                    TargetPath = "quantity",
                    Transform = TransformType.ToNumber
                }
            ]
        };

        var engine = CreateEngine();

        using var input = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        using var output = new MemoryStream();

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.True(result.IsSuccess);
        Assert.True(output.Length > 0);
    }

    [Fact]
    public void JsonToXml_ShouldExecuteCompletePipeline()
    {
        const string json =
            """
            {
              "product": {
                "id": "123",
                "name": "Keyboard",
                "price": 99.90,
                "category": "hardware"
              }
            }
            """;

        var template = new TemplateDefinition
        {
            Id = "json-to-xml",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "product.name",
                    TargetPath = "product.name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "product.price",
                    TargetPath = "product.price",
                    Transform = TransformType.ToNumber
                },
                new FieldMapping
                {
                    SourcePath = "product.category",
                    TargetPath = "product.category"
                }
            ]
        };

        var engine = CreateEngine();

        using var input = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        using var output = new MemoryStream();

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.True(result.IsSuccess);
        Assert.True(output.Length > 0);

        output.Position = 0;

        using var reader = new StreamReader(output);

        var xml = reader.ReadToEnd();

        Assert.Contains("<product>", xml);
        Assert.Contains("<id>123</id>", xml);
        Assert.Contains("<name>Keyboard</name>", xml);
        Assert.Contains("<price>99.90</price>", xml);
        Assert.Contains("<category>hardware</category>", xml);
    }

    [Fact]
    public void XmlToXml_ShouldExecuteCompletePipeline()
    {
        const string xml =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <PurchaseOrder>
              <OrderID>PO-001</OrderID>
              <Supplier>
                <Name>ACME</Name>
              </Supplier>
              <Buyer>
                <Name>WARP Corp</Name>
              </Buyer>
              <Item>
                <SKU>ABC-001</SKU>
                <Description>Keyboard</Description>
                <Quantity>2</Quantity>
                <UnitPrice>99.90</UnitPrice>
              </Item>
            </PurchaseOrder>
            """;

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
                    TargetPath = "cXML.Request.OrderRequest.OrderID",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Supplier.Name",
                    TargetPath = "cXML.Request.OrderRequest.Supplier.Name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Buyer.Name",
                    TargetPath = "cXML.Request.OrderRequest.Buyer.Name",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "Item.SKU",
                    TargetPath = "cXML.Request.OrderRequest.ItemOut.SKU",
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
                    Required = true
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

        var engine = CreateEngine();

        using var input = new MemoryStream(
            Encoding.UTF8.GetBytes(xml));

        using var output = new MemoryStream();

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.True(result.IsSuccess);
        Assert.True(output.Length > 0);

        output.Position = 0;

        using var reader = new StreamReader(output);

        var generatedXml = reader.ReadToEnd();

        Assert.Contains("<cXML>", generatedXml);
        Assert.Contains("<OrderID>PO-001</OrderID>", generatedXml);
        Assert.Contains("<Name>ACME</Name>", generatedXml);
        Assert.Contains("<SKU>ABC-001</SKU>", generatedXml);
        Assert.Contains("<Description>Keyboard</Description>", generatedXml);
        Assert.Contains("<Quantity>2</Quantity>", generatedXml);
        Assert.Contains("<UnitPrice>99.90</UnitPrice>", generatedXml);
    }

    [Fact]
    public void MissingRequiredField_ShouldStopPipeline()
    {
        const string json =
            """
            {
              "product": {
                "name": "Keyboard"
              }
            }
            """;

        var template = new TemplateDefinition
        {
            Id = "json-to-xml",
            Version = 1,
            SourceFormat = "json",
            TargetFormat = "xml",
            Mappings =
            [
                new FieldMapping
                {
                    SourcePath = "product.id",
                    TargetPath = "product.id",
                    Required = true
                },
                new FieldMapping
                {
                    SourcePath = "product.name",
                    TargetPath = "product.name",
                    Required = true
                }
            ]
        };

        var engine = CreateEngine();

        using var input = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        using var output = new MemoryStream();

        var result = engine.Execute(
            input,
            output,
            template);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Validation.Errors);

        Assert.Equal(0, output.Length);
    }
}