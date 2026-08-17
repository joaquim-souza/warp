using Warp.Core.Engine;
using Warp.Core.Parsing;
using Warp.Core.Registry;
using Warp.Core.Serialization;
using Warp.Core.Templates;
using Warp.Core.Transform;
using Warp.Excel;

namespace Warp.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            // Comandos administrativos do WARP
            if (IsTemplateCreateCommand(args))
            {
                return TemplateCommand.Create(
                    args.Skip(2).ToArray());
            }

            if (IsTemplateValidateCommand(args))
            {
                return TemplateCommand.Validate(
                    args.Skip(2).ToArray());
            }

            if (IsTemplateAddMappingCommand(args))
            {
                return TemplateCommand.AddMapping(
                    args.Skip(2).ToArray());
            }

            if (IsTemplateAddCollectionCommand(args))
            {
                return TemplateCommand.AddCollection(
                    args.Skip(2).ToArray());
            }

            var options =
                CliOptions.Parse(args);

            if (options.ShowHelp)
            {
                CliOptions.PrintHelp();
                return 0;
            }

            if (File.Exists(options.Output) &&
                !options.Force)
            {
                Console.Error.WriteLine(
                    $"Erro: arquivo de saída já existe: {options.Output}");

                Console.Error.WriteLine(
                    "Use --force para sobrescrever.");

                return 2;
            }

            if (!File.Exists(options.Input))
            {
                Console.Error.WriteLine(
                    $"Erro: arquivo de entrada não encontrado: {options.Input}");

                return 2;
            }

            if (!File.Exists(options.Template))
            {
                Console.Error.WriteLine(
                    $"Erro: template não encontrado: {options.Template}");

                return 2;
            }

            var parsers =
                new ParserRegistry(
                [
                    new CsvParser(),
                    new JsonParser(),
                    new XmlParser()
                ]);

            var serializers =
                new SerializerRegistry(
                [
                    new XmlCanonicalSerializer(),
                    new ExcelSerializer()
                ]);

            var transformer =
                new Transformer();

            var engine =
                new WarpEngine(
                    parsers,
                    serializers,
                    transformer);

            var loader =
                new TemplateLoader();

            var template =
                loader.Load(
                    options.Template);

            using var input =
                File.OpenRead(
                    options.Input);

            var outputDirectory =
                Path.GetDirectoryName(
                    Path.GetFullPath(
                        options.Output));

            if (!string.IsNullOrEmpty(
                outputDirectory))
            {
                Directory.CreateDirectory(
                    outputDirectory);
            }

            using var output =
                File.Create(
                    options.Output);

            var result =
                engine.Execute(
                    input,
                    output,
                    template);

            if (!result.IsSuccess)
            {
                Console.Error.WriteLine(
                    "Conversão falhou.");

                foreach (var error in
                         result.Validation.Errors)
                {
                    Console.Error.WriteLine(
                        $"[{error.Path}] {error.Message}");
                }

                return 1;
            }

            Console.WriteLine(
                "Conversão concluída.");

            Console.WriteLine(
                $"Input:    {options.Input}");

            Console.WriteLine(
                $"Template: {options.Template}");

            Console.WriteLine(
                $"Output:   {options.Output}");

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(
                $"Erro de argumento: {ex.Message}");

            return 2;
        }
        catch (KeyNotFoundException ex)
        {
            Console.Error.WriteLine(
                $"Erro de configuração: {ex.Message}");

            return 2;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine(
                $"Erro de configuração: {ex.Message}");

            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Erro inesperado: {ex.Message}");

            return 3;
        }
    }

    private static bool IsTemplateCreateCommand(
        string[] args)
    {
        return args.Length >= 2
            && args[0].Equals(
                "template",
                StringComparison.OrdinalIgnoreCase)
            && args[1].Equals(
                "create",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTemplateValidateCommand(
        string[] args)
    {
        return args.Length >= 2
            && args[0].Equals(
                "template",
                StringComparison.OrdinalIgnoreCase)
            && args[1].Equals(
                "validate",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTemplateAddMappingCommand(
        string[] args)
    {
        return args.Length >= 2
            && args[0].Equals(
                "template",
                StringComparison.OrdinalIgnoreCase)
            && args[1].Equals(
                "add-mapping",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTemplateAddCollectionCommand(
        string[] args)
    {
        return args.Length >= 2
            && args[0].Equals(
                "template",
                StringComparison.OrdinalIgnoreCase)
            && args[1].Equals(
                "add-collection",
                StringComparison.OrdinalIgnoreCase);
    }
}