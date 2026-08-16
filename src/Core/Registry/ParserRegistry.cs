using Warp.Core.Parsing;

namespace Warp.Core.Registry;

/// <summary>
/// Registro dos parsers disponíveis no WARP.
///
/// O registry desacopla o engine das implementações concretas:
/// o restante do sistema pede "csv", "json" ou "xml" sem precisar
/// conhecer CsvParser, JsonParser ou XmlParser diretamente.
/// </summary>
public sealed class ParserRegistry
{
    private readonly Dictionary<string, ICanonicalParser> _parsers =
        new(StringComparer.OrdinalIgnoreCase);

    public ParserRegistry(IEnumerable<ICanonicalParser> parsers)
    {
        foreach (var parser in parsers)
        {
            Register(parser);
        }
    }

    public void Register(ICanonicalParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        if (string.IsNullOrWhiteSpace(parser.FormatName))
        {
            throw new ArgumentException(
                "Parser deve possuir um FormatName.",
                nameof(parser));
        }

        if (!_parsers.TryAdd(parser.FormatName, parser))
        {
            throw new InvalidOperationException(
                $"Já existe um parser registrado para o formato '{parser.FormatName}'.");
        }
    }

    public ICanonicalParser Get(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException(
                "Formato não pode ser vazio.",
                nameof(format));
        }

        if (!_parsers.TryGetValue(format, out var parser))
        {
            throw new KeyNotFoundException(
                $"Nenhum parser registrado para o formato '{format}'.");
        }

        return parser;
    }

    public bool Contains(string format) =>
        !string.IsNullOrWhiteSpace(format) &&
        _parsers.ContainsKey(format);

    public IReadOnlyCollection<string> Formats =>
        _parsers.Keys.ToList().AsReadOnly();
}