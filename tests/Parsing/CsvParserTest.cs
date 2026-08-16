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
    }}

   
public sealed class CsvParserMalformedTests
{
    [Fact]
    public void Parse_ShouldFillMissingColumns()
    {
        const string csv =
            "id,name,price\n" +
            "1,Product";

        using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(csv));

        var parser =
            new CsvParser();

        var document =
            parser.Parse(stream);

        var row =
            document.Root.Child("row");

        Assert.NotNull(row);

        Assert.Equal(
            "1",
            row!.Child("id")!.Value);

        Assert.Equal(
            "Product",
            row.Child("name")!.Value);

        Assert.Equal(
            string.Empty,
            row.Child("price")!.Value);
    }

    [Fact]
    public void Parse_ShouldRejectExtraColumns()
    {
        const string csv =
            "id,name\n" +
            "1,Product,Unexpected";

        using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(csv));

        var parser =
            new CsvParser();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(stream));

        Assert.Contains(
            "possui 3 colunas",
            exception.Message);

        Assert.Contains(
            "cabeçalho possui 2",
            exception.Message);
    }

    [Fact]
    public void Parse_ShouldIgnoreEmptyLines()
    {
        const string csv =
            "id,name\n" +
            "\n" +
            "1,Product\n" +
            "\n";

        using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(csv));

        var parser =
            new CsvParser();

        var document =
            parser.Parse(stream);

        Assert.Equal(
            1,
            document.Root.Children.Count);
    }

    [Fact]
    public void Parse_ShouldRejectUnclosedQuotes()
    {
        const string csv =
            "id,name\n" +
            "1,\"Product";

        using var stream =
            new MemoryStream(
                Encoding.UTF8.GetBytes(csv));

        var parser =
            new CsvParser();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () => parser.Parse(stream));

        Assert.Contains(
            "aspas não fechadas",
            exception.Message);
    }

    [Fact]
public void Parse_ShouldReadUtf8()
{
    const string csv =
        "id,name\n" +
        "1,João";

    using var stream =
        new MemoryStream(
            Encoding.UTF8.GetBytes(csv));

    var parser =
        new CsvParser();

    var document =
        parser.Parse(stream);

    var value =
        document.Root
            .Child("row")!
            .Child("name")!
            .Value;

    Assert.Equal(
        "João",
        value);
}

[Fact]
public void Parse_ShouldReadUtf8WithBom()
{
    const string csv =
        "id,name\n" +
        "1,São Paulo";

    var encoding =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true);

    using var stream =
        new MemoryStream(
            encoding.GetBytes(csv));

    var parser =
        new CsvParser();

    var document =
        parser.Parse(stream);

    var row =
        document.Root.Child("row");

    Assert.NotNull(row);

    Assert.Equal(
        "São Paulo",
        row!.Child("name")!.Value);
    }
}
