using ClosedXML.Excel;
using Warp.Core.Model;
using Warp.Core.Serialization;

namespace Warp.Excel;

/// <summary>
/// Serializa um CanonicalDocument para XLSX.
///
/// O adapter não conhece CSV, JSON ou XML.
/// Ele simplesmente recebe a árvore canônica produzida pelo Core
/// e transforma seus registros em uma planilha Excel.
/// </summary>
public sealed class ExcelSerializer : ICanonicalSerializer
{
    public string FormatName => "xlsx";

    public void Serialize(CanonicalDocument document, Stream output)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Data");

        var records = GetRecords(document.Root);

        if (records.Count == 0)
        {
            workbook.SaveAs(output);
            return;
        }

        var columns = GetColumns(records);

        WriteHeader(worksheet, columns);
        WriteRecords(worksheet, records, columns);

        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(output);
    }

    private static List<CanonicalNode> GetRecords(CanonicalNode root)
    {
        // Transformer cria "records" quando RecordsPath está definido.
        var records = root.ChildrenNamed("record").ToList();

        // Caso seja um documento único.
        if (records.Count == 0 && root.Name == "record")
        {
            records.Add(root);
        }

        return records;
    }

    private static List<string> GetColumns(
        IReadOnlyList<CanonicalNode> records)
    {
        var columns = new List<string>();

        foreach (var record in records)
        {
            foreach (var child in record.Children)
            {
                if (!columns.Contains(child.Name, StringComparer.OrdinalIgnoreCase))
                {
                    columns.Add(child.Name);
                }
            }
        }

        return columns;
    }

    private static void WriteHeader(
        IXLWorksheet worksheet,
        IReadOnlyList<string> columns)
    {
        for (int i = 0; i < columns.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = columns[i];
        }
    }

    private static void WriteRecords(
        IXLWorksheet worksheet,
        IReadOnlyList<CanonicalNode> records,
        IReadOnlyList<string> columns)
    {
        for (int rowIndex = 0; rowIndex < records.Count; rowIndex++)
        {
            var record = records[rowIndex];

            for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var columnName = columns[columnIndex];

                var node = record.Child(columnName);

                if (node?.Value is not null)
                {
                    worksheet.Cell(
                        rowIndex + 2,
                        columnIndex + 1).Value = node.Value;
                }
            }
        }
    }
}