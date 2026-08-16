namespace Warp.Cli;

public sealed class CliOptions
{
    public string Input { get; private set; } = "";
    public string Output { get; private set; } = "";
    public string Template { get; private set; } = "";

    public bool Force { get; private set; }
    public bool ShowHelp { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
        {
            throw new ArgumentException(
                "Nenhum argumento informado. Use --help para ajuda.");
        }

        if (args.Any(arg =>
            string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)))
        {
            return new CliOptions
            {
                ShowHelp = true
            };
        }

        var options = new CliOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--input":
                case "-i":
                    options.Input = ReadValue(args, ref i, "--input");
                    break;

                case "--output":
                case "-o":
                    options.Output = ReadValue(args, ref i, "--output");
                    break;

                case "--template":
                case "-t":
                    options.Template = ReadValue(args, ref i, "--template");
                    break;

                case "--force":
                case "-f":
                    options.Force = true;
                    break;

                default:
                    throw new ArgumentException(
                        $"Argumento desconhecido: '{args[i]}'.");
            }
        }

        Validate(options);

        return options;
    }

    private static string ReadValue(
    string[] args,
    ref int index,
    string argument)
{
    if (index + 1 >= args.Length)
    {
        throw new ArgumentException(
            $"O argumento '{argument}' exige um valor.");
    }

    var value = args[index + 1];

    if (string.IsNullOrWhiteSpace(value) ||
        value.StartsWith("-", StringComparison.Ordinal))
    {
        throw new ArgumentException(
            $"O argumento '{argument}' exige um valor.");
    }

    index++;

    return value;
}

    private static void Validate(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Input))
        {
            throw new ArgumentException(
                "Informe o arquivo de entrada com --input.");
        }

        if (string.IsNullOrWhiteSpace(options.Output))
        {
            throw new ArgumentException(
                "Informe o arquivo de saída com --output.");
        }

        if (string.IsNullOrWhiteSpace(options.Template))
        {
            throw new ArgumentException(
                "Informe o template com --template.");
        }
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
        WARP - Universal Data Transformation Engine

        Uso:

          warp --input <arquivo> --output <arquivo> --template <arquivo>

        Opções:

          -i, --input       Arquivo de entrada
          -o, --output      Arquivo de saída
          -t, --template    Template YAML
          -f, --force       Sobrescreve o arquivo de saída
          -h, --help        Exibe esta ajuda

        Exemplos:

          warp --input samples/csv/products.csv
               --output samples/output.xlsx
               --template templates/csv-to-xlsx.v1.yaml

          warp --input samples/json/product.json
               --output samples/output.xml
               --template templates/json-to-xml.v1.yaml
        """);
    }
}