using System.Text;
using Warp.Core.Parsing;

namespace Tests.Parsing;

public sealed class CsvParserTests
{
    [Fact]
    public void Parse_ShouldCreateRowsAndColumns()
    {
        const string csv =
            "sku,name,price\n" +
            "001,Keyboard,99.90\n" +
            "002,Mouse,49.90";

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        var parser = new CsvParser();

        var document = parser.Parse(stream);

        Assert.Equal("csv", document.SourceFormat);
        Assert.Equal("rows", document.Root.Name);

        var rows = document.Root.ChildrenNamed("row").ToList();

        Assert.Equal(2, rows.Count);

        Assert.Equal("001", rows[0].Child("sku")?.Value);
        Assert.Equal("Keyboard", rows[0].Child("name")?.Value);
        Assert.Equal("99.90", rows[0].Child("price")?.Value);
    }

    [Fact]
    public void Parse_ShouldPreserveQuotedComma()
    {
        const string csv =
            "sku,name\n" +
            "001,\"Keyboard, Mechanical\"";

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        var document = new CsvParser().Parse(stream);

        var row = document.Root.Child("row");

        Assert.Equal(
            "Keyboard, Mechanical",
            row?.Child("name")?.Value);
    }

    [Fact]
    public void Parse_ShouldSupportQuotedLineBreak()
    {
        const string csv =
            "sku,description\n" +
            "001,\"line one\nline two\"";

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

        var document = new CsvParser().Parse(stream);

        var row = document.Root.Child("row");

        Assert.Equal(
            "line one\nline two",
            row?.Child("description")?.Value);
    }
}