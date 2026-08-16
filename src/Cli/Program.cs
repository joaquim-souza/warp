using Warp.Core.Engine;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;
using Warp.Excel;

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

if (!string.Equals(args[0], "convert", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine(
        $"Comando desconhecido: '{args[0]}'");

    PrintUsage();
    return 1;
}

if (!TryParseArguments(
        args,
        out var inputPath,
        out var templatePath,
        out var outputPath))
{
    return 1;
}

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine(
        $"Arquivo de entrada não encontrado: {inputPath}");

    return 1;
}

if (!File.Exists(templatePath))
{
    Console.Error.WriteLine(
        $"Template não encontrado: {templatePath}");

    return 1;
}

try
{
    var templateLoader = new TemplateLoader();
    var template = templateLoader.Load(templatePath);

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

    var transformer = new Transformer();

    var engine = new WarpEngine(
        parsers,
        serializers,
        transformer);

    Directory.CreateDirectory(
        Path.GetDirectoryName(
            Path.GetFullPath(outputPath))!);

    await using var input = File.OpenRead(inputPath);
    await using var output = File.Create(outputPath);

    var result = engine.Execute(
        input,
        output,
        template);

    if (!result.IsSuccess)
    {
        Console.Error.WriteLine("Conversão falhou.");

        foreach (var error in result.Validation.Errors)
        {
            Console.Error.WriteLine(
                $"[{error.Path}] {error.Message}");
        }

        return 1;
    }

    Console.WriteLine("Conversão concluída.");
    Console.WriteLine($"Template: {template.Id}.v{template.Version}");
    Console.WriteLine($"Entrada:  {inputPath}");
    Console.WriteLine($"Saída:    {outputPath}");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        $"Erro durante a conversão: {ex.Message}");

    return 1;
}

static bool TryParseArguments(
    string[] args,
    out string inputPath,
    out string templatePath,
    out string outputPath)
{
    inputPath = "";
    templatePath = "";
    outputPath = "";

    for (int i = 1; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--input":
                if (!TryReadValue(args, ref i, out inputPath))
                    return false;
                break;

            case "--template":
                if (!TryReadValue(args, ref i, out templatePath))
                    return false;
                break;

            case "--output":
                if (!TryReadValue(args, ref i, out outputPath))
                    return false;
                break;

            case "--help":
            case "-h":
                PrintUsage();
                return false;

            default:
                Console.Error.WriteLine(
                    $"Argumento desconhecido: {args[i]}");

                PrintUsage();
                return false;
        }
    }

    if (string.IsNullOrWhiteSpace(inputPath) ||
        string.IsNullOrWhiteSpace(templatePath) ||
        string.IsNullOrWhiteSpace(outputPath))
    {
        Console.Error.WriteLine(
            "Os argumentos --input, --template e --output são obrigatórios.");

        PrintUsage();
        return false;
    }

    return true;
}

static bool TryReadValue(
    string[] args,
    ref int index,
    out string value)
{
    value = "";

    if (index + 1 >= args.Length)
    {
        Console.Error.WriteLine(
            $"Valor ausente para '{args[index]}'.");

        return false;
    }

    value = args[++index];

    if (string.IsNullOrWhiteSpace(value))
    {
        Console.Error.WriteLine(
            $"Valor vazio para '{args[index - 1]}'.");

        return false;
    }

    return true;
}

static void PrintUsage()
{
    Console.WriteLine("""
        WARP - Universal M2M Transformation Engine

        Uso:

          warp convert \
            --input <arquivo> \
            --template <template.yaml> \
            --output <arquivo>

        Exemplos:

          warp convert \
            --input samples/csv/products.csv \
            --template templates/csv-to-xlsx.v1.yaml \
            --output output/products.xlsx

          warp convert \
            --input samples/json/product.json \
            --template templates/json-to-xml.v1.yaml \
            --output output/product.xml

          warp convert \
            --input samples/xml/purchase-order.xml \
            --template templates/xml-to-cxml.v1.yaml \
            --output output/order.xml
        """);
}