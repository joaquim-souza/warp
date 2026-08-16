using System.Text;
using Warp.Core.Model;

namespace Warp.Core.Parsing;

/// <summary>
/// Parser CSV com cabeçalho. Produz uma raiz "rows" com um filho "row" por
/// linha, e dentro de cada "row" um filho por coluna (nome = cabeçalho,
/// valor = célula).
/// <para/>
/// <b>LIMITAÇÃO CONHECIDA, documentada de propósito:</b> implementação RFC 4180
/// simplificada — lida com campos entre aspas contendo vírgula ou quebra de
/// linha, mas não cobre 100% dos casos extremos da RFC (aspas escapadas
/// aninhadas de forma incomum, por exemplo). Para CSV gerado por Excel/Google
/// Sheets (o caso de uso real do WARP) isso cobre o que aparece na prática.
/// Evolução natural: trocar por CsvHelper se algum CSV de origem realmente
/// exigir 100% de conformidade RFC.
/// </summary>
public sealed class CsvParser : ICanonicalParser
{
    public string FormatName => "csv";

    public CanonicalDocument Parse(Stream input)
    {
        using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
        var lines = ReadCsvRecords(reader);

        var root = new CanonicalNode("rows");
        if (lines.Count == 0)
        {
            return new CanonicalDocument(root, FormatName);
        }

        var headers = lines[0];
        for (int i = 1; i < lines.Count; i++)
        {
            var fields = lines[i];
            var rowNode = new CanonicalNode("row");
            for (int col = 0; col < headers.Count; col++)
            {
                string value = col < fields.Count ? fields[col] : "";
                rowNode.AddChild(headers[col].Trim(), value);
            }
            root.AddChild(rowNode);
        }

        return new CanonicalDocument(root, FormatName);
    }

    /// <summary>Lê registros respeitando campos entre aspas (que podem conter vírgula/quebra de linha).</summary>
    private static List<List<string>> ReadCsvRecords(TextReader reader)
    {
        var records = new List<List<string>>();
        var currentRecord = new List<string>();
        var currentField = new StringBuilder();
        bool insideQuotes = false;
        int ch;
        bool anyContentInRecord = false;

        while ((ch = reader.Read()) != -1)
        {
            char c = (char)ch;
            anyContentInRecord = true;

            if (insideQuotes)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        currentField.Append('"');
                    }
                    else
                    {
                        insideQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }
                continue;
            }

            switch (c)
            {
                case '"':
                    insideQuotes = true;
                    break;
                case ',':
                    currentRecord.Add(currentField.ToString());
                    currentField.Clear();
                    break;
                case '\r':
                    break; // ignora, trata \n como o delimitador de linha real
                case '\n':
                    currentRecord.Add(currentField.ToString());
                    currentField.Clear();
                    records.Add(currentRecord);
                    currentRecord = new List<string>();
                    anyContentInRecord = false;
                    break;
                default:
                    currentField.Append(c);
                    break;
            }
        }

        if (anyContentInRecord || currentField.Length > 0 || currentRecord.Count > 0)
        {
            currentRecord.Add(currentField.ToString());
            records.Add(currentRecord);
        }

        return records;
    }
}