using ClosedXML.Excel;
using Leeve.Client.Common;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Client.Evaluations;

public sealed class EvaluationExportService
{
    public Task<Result> ExportAsync(List<Evaluation> evaluations, string path)
    {
        return Task.Run(() => Export(evaluations, path));
    }

    private Result Export(List<Evaluation> evaluations, string filePath)
    {
        try
        {
            IXLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Evaluation Summary");

            ws.WriteReportHeader(evaluations);
            ws.WriteReportBody(evaluations);
            ws.ConcludeReport();
            ws.SetPageSettings(XLPageOrientation.Portrait, 10);

            wb.Save(filePath);
            return new Result();
        }
        catch (Exception)
        {
            return new Result(new Exception("Failed to export report"));
        }
    }
}