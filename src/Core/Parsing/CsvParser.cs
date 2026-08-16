using System.Text;
using Warp.Core.Model;

namespace Warp.Core.Parsing;

public sealed class CsvParser : ICanonicalParser
{
    public string FormatName => "csv";

    public CanonicalDocument Parse(Stream input)
    {
        return Parse(
            input,
            "utf-8");
    }

    public CanonicalDocument Parse(
        Stream input,
        string encodingName)
    {
        var encoding =
            ResolveEncoding(
                encodingName);

        using var reader =
            new StreamReader(
                input,
                encoding,
                detectEncodingFromByteOrderMarks:
                    IsUtf8(encoding),
                leaveOpen: true);

        var records =
            ReadRecords(reader);

        if (records.Count == 0)
        {
            return new CanonicalDocument(
                new CanonicalNode("rows"),
                "csv");
        }

        var headers =
            ParseLine(records[0]);

        if (headers.Count == 0)
        {
            return new CanonicalDocument(
                new CanonicalNode("rows"),
                "csv");
        }

        var root =
            new CanonicalNode("rows");

        foreach (var record in records.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(record))
            {
                continue;
            }

            var values =
                ParseLine(record);

            if (values.Count > headers.Count)
            {
                throw new InvalidOperationException(
                    $"CSV inválido: linha possui {values.Count} " +
                    $"colunas, mas o cabeçalho possui {headers.Count}.");
            }

            var row =
                root.AddChild("row");

            for (var i = 0; i < headers.Count; i++)
            {
                var value =
                    i < values.Count
                        ? values[i]
                        : string.Empty;

                row.AddChild(
                    headers[i],
                    value);
            }
        }

        return new CanonicalDocument(
            root,
            "csv");
    }

    private static Encoding ResolveEncoding(
        string encodingName)
    {
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);

        if (string.IsNullOrWhiteSpace(
            encodingName))
        {
            return Encoding.UTF8;
        }

        return encodingName
            .Trim()
            .ToLowerInvariant() switch
        {
            "utf-8" =>
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false),

            "utf8" =>
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false),

            "windows-1252" =>
                Encoding.GetEncoding(1252),

            "cp1252" =>
                Encoding.GetEncoding(1252),

            "iso-8859-1" =>
                Encoding.GetEncoding(28591),

            "latin1" =>
                Encoding.GetEncoding(28591),

            _ =>
                throw new ArgumentException(
                    $"Encoding não suportado: '{encodingName}'. " +
                    "Use utf-8, windows-1252 ou iso-8859-1.")
        };
    }

    private static bool IsUtf8(
        Encoding encoding)
    {
        return encoding.CodePage == Encoding.UTF8.CodePage;
    }

    private static List<string> ReadRecords(
        StreamReader reader)
    {
        var records =
            new List<string>();

        var current =
            new StringBuilder();

        var quoted = false;

        while (!reader.EndOfStream)
        {
            var line =
                reader.ReadLine() ?? string.Empty;

            if (current.Length > 0)
            {
                current.Append('\n');
            }

            current.Append(line);

            quoted =
                UpdateQuoteState(
                    line,
                    quoted);

            if (!quoted)
            {
                records.Add(
                    current.ToString());

                current.Clear();
            }
        }

        if (current.Length > 0)
        {
            throw new InvalidOperationException(
                "CSV inválido: aspas não fechadas.");
        }

        return records;
    }

    private static bool UpdateQuoteState(
        string line,
        bool quoted)
    {
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] != '"')
            {
                continue;
            }

            if (quoted &&
                i + 1 < line.Length &&
                line[i + 1] == '"')
            {
                i++;
                continue;
            }

            quoted = !quoted;
        }

        return quoted;
    }

    private static List<string> ParseLine(
        string line)
    {
        var values =
            new List<string>();

        var current =
            new StringBuilder();

        var quoted = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character =
                line[i];

            if (character == '"')
            {
                if (quoted &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                    continue;
                }

                quoted = !quoted;
                continue;
            }

            if (character == ',' && !quoted)
            {
                values.Add(
                    current.ToString());

                current.Clear();

                continue;
            }

            current.Append(character);
        }

        if (quoted)
        {
            throw new InvalidOperationException(
                "CSV inválido: aspas não fechadas.");
        }

        values.Add(
            current.ToString());

        return values;
    }
}