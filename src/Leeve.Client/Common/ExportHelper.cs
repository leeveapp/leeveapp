using System.Diagnostics;
using ClosedXML.Excel;

namespace Leeve.Client.Common;

public static class ExportHelper
{
    internal static string AccountingFormat => "_(* #,##0.00_);_(* (#,##0.00);_(* \" - \"??_);_(@_)";

    internal static void SetPageSettings(this IXLWorksheet ws, XLPageOrientation orientation, int headerRow)
    {
        ws.PageSetup.Margins
            .SetLeft(0.50)
            .SetTop(0.50)
            .SetRight(0.50)
            .SetBottom(0.50);

        ws.PageSetup
            .SetPaperSize(XLPaperSize.A4Paper)
            .SetPageOrientation(orientation)
            .SetCenterHorizontally(true)
            .SetRowsToRepeatAtTop(headerRow, headerRow);
    }

    internal static void Save(this IXLWorkbook wb, string filePath)
    {
        var file = Path.GetFileNameWithoutExtension(filePath);
        var path = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(path) || string.IsNullOrEmpty(path))
            throw new InvalidOperationException("Invalid file path.");

        var excelFile = $"{Path.Combine(path, file)}.xlsx";
        wb.SaveAs(excelFile);
        OpenExcelFile(excelFile);
    }

    private static void OpenExcelFile(string excelFile)
    {
        new Process { StartInfo = new ProcessStartInfo(excelFile) { UseShellExecute = true } }.Start();
    }
}