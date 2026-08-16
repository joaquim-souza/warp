using Warp.Core.Engine;
using Warp.Core.Registry;
using Warp.Core.Templates;
using Warp.Excel;

namespace Warp.Cli.Commands;

public sealed class ConvertCommand
{
    public int Execute(
        string inputPath,
        string templatePath,
        string outputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath) ||
            string.IsNullOrWhiteSpace(templatePath) ||
            string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Error.WriteLine(
                "Uso: warp convert <input> <template> <output>");

            return CliResult.InvalidArguments;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine(
                $"Arquivo de entrada não encontrado: {inputPath}");

            return CliResult.InputNotFound;
        }

        if (!File.Exists(templatePath))
        {
            Console.Error.WriteLine(
                $"Template não encontrado: {templatePath}");

            return CliResult.TemplateError;
        }

        try
        {
            var loader = new TemplateLoader();
            var template = loader.Load(templatePath);

            var parsers = new ParserRegistry(
            [
                new Warp.Core.Parsing.CsvParser(),
                new Warp.Core.Parsing.JsonParser(),
                new Warp.Core.Parsing.XmlParser()
            ]);

            var serializers = new SerializerRegistry(
            [
                new Warp.Core.Serialization.XmlCanonicalSerializer(),
                new ExcelSerializer()
            ]);

            var engine = new WarpEngine(
                parsers,
                serializers,
                new Warp.Core.Transform.Transformer());

            var outputDirectory = Path.GetDirectoryName(
                Path.GetFullPath(outputPath));

            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            using var input = File.OpenRead(inputPath);
            using var output = File.Create(outputPath);

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
                        $"{error.Path}: {error.Message}");
                }

                return CliResult.ConversionError;
            }

            Console.WriteLine("Conversão concluída.");
            Console.WriteLine($"Template: {template.Id}.v{template.Version}");
            Console.WriteLine($"Entrada:  {inputPath}");
            Console.WriteLine($"Saída:    {outputPath}");

            return CliResult.Success;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine(
                $"Erro no template: {ex.Message}");

            return CliResult.TemplateError;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Erro durante a conversão: {ex.Message}");

            return CliResult.ConversionError;
        }
    }
}