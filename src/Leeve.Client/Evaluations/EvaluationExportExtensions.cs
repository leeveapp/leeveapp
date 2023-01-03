using ClosedXML.Excel;
using Leeve.Client.Common;
using Leeve.Core.Evaluations;
using Leeve.Core.Questionnaires;

namespace Leeve.Client.Evaluations;

public static class EvaluationExportExtensions
{
    public static void WriteReportHeader(this IXLWorksheet ws, List<Evaluation> evaluations)
    {
        var evaluation = evaluations.First();

        ws.Cell(1, 1).SetValue("Le'eve Evaluation Report")
            .Style
            .Font.SetBold()
            .Font.SetFontSize(16)
            .Font.SetFontColor(XLColor.FromHtml("#004C70"));

        ws.Cell(3, 1).Value = "Title:";
        ws.Cell(3, 2).SetValue(evaluation.Title)
            .Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        ws.Range(3, 2, 3, 6).Merge();

        ws.Cell(4, 1).Value = "Description:";
        ws.Cell(4, 2).SetValue(evaluation.Description)
            .Style
            .Font.SetBold()
            .Font.SetFontSize(10)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        ws.Range(4, 2, 4, 6).Merge();

        ws.Cell(5, 1).Value = "Respondents:";
        ws.Cell(5, 2).SetValue(evaluations.Count)
            .Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        ws.Cell(7, 1).Value = "Questionnaire:";
        ws.Cell(7, 2).SetValue(evaluation.Questionnaire.Title)
            .Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        ws.Range(7, 2, 7, 6).Merge();

        ws.Cell(8, 1).Value = "Description:";
        ws.Cell(8, 2).SetValue(evaluation.Questionnaire.Description)
            .Style
            .Font.SetBold()
            .Font.SetFontSize(10)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
        ws.Range(8, 2, 8, 6).Merge();

        ws.Range(10, 1, 10, 6).Style
            .Font.SetBold()
            .Font.SetFontColor(XLColor.White)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText(true)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#004C70"))
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromHtml("#004C70"));

        ws.Cell(10, 1).Value = "Details";
        ws.Range(10, 1, 10, 2).Merge()
            .Style
            .Font.SetBold()
            .NumberFormat.SetFormat(ExportHelper.AccountingFormat)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        ws.Cell(10, 3).SetValue("Weighted Score").Style.Font.SetFontSize(9);
        ws.Cell(10, 4).SetValue("No. of Responses").Style.Font.SetFontSize(9);
        ws.Cell(10, 5).SetValue("% of Responses").Style.Font.SetFontSize(9);
        ws.Cell(10, 6).SetValue("Average Score").Style.Font.SetFontSize(9);

        ws.Row(10).Height = 35;
    }

    public static void WriteReportBody(this IXLWorksheet ws, List<Evaluation> evaluations)
    {
        var sample = evaluations.First().Questionnaire.Methodologies;
        var methodologies = evaluations.SelectMany(x => x.Questionnaire.Methodologies).ToList();

        var methodologyRow = 11;
        foreach (var methodology in sample)
        {
            var list = methodologies.Where(m => m.Title == methodology.Title).ToList();
            var questionRows = new List<int>();

            var row = methodologyRow + 1;
            ws.WriteQuestions(list, questionRows, ref row);
            ws.WriteMethodologyResult(methodology.Title, questionRows, methodologyRow);

            methodologyRow = row - 1;
        }
    }

    private static void WriteQuestions(this IXLWorksheet ws, IReadOnlyCollection<Methodology> list,
        ICollection<int> questionRows, ref int row)
    {
        var questions = list.First().Questions;

        var questionRow = row++;
        foreach (var question in questions)
        {
            WriteAnswers(ws, list, question, ref row);

            ws.Range(questionRow, 1, questionRow, 2).Merge()
                .Style
                .Alignment.SetWrapText();
            ws.Range(questionRow, 3, questionRow, 6)
                .Style
                .Font.SetBold()
                .Font.SetItalic()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetWrapText();

            ws.Range(questionRow, 1, row - 1, 6)
                .Style
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorderColor(XLColor.FromHtml("#004C70"));

            ws.Cell(questionRow, 1).SetValue(question.Title)
                .Style
                .Font.SetBold()
                .Font.SetItalic()
                .Alignment.SetIndent(1)
                .NumberFormat.SetFormat(ExportHelper.AccountingFormat);

            ws.Cell(questionRow, 3).SetFormulaA1($"=MAX(C{questionRow + 1}:C{row - 1})");
            ws.Cell(questionRow, 6).SetFormulaA1($"=SUM(F{questionRow + 1}:F{row - 1})")
                .Style
                .NumberFormat.SetFormat("0.00");

            questionRows.Add(questionRow);
            questionRow = row++;
        }
    }

    private static void WriteAnswers(this IXLWorksheet ws, IReadOnlyCollection<Methodology> list, Question question, ref int row)
    {
        var answers = list.SelectMany(m => m.Questions
                .Where(q => q.Title == question.Title)
                .SelectMany(q => q.Answers))
            .GroupBy(a => a.Title).Select(x => new AnswerResult
            {
                Title = x.Key,
                WeightedScore = x.First().Weight,
                Count = x.Count(a => a.Selected)
            })
            .OrderBy(a => a.WeightedScore)
            .ToList();

        var answerRow = row++;
        foreach (var answer in answers)
        {
            ws.Range(answerRow, 1, answerRow, 2).Merge()
                .Style
                .Alignment.SetWrapText();
            ws.Range(answerRow, 3, answerRow, 6)
                .Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetWrapText();

            ws.Cell(answerRow, 1).SetValue(answer.Title)
                .Style
                .Alignment.SetIndent(2)
                .NumberFormat.SetFormat(ExportHelper.AccountingFormat);

            ws.Cell(answerRow, 3).SetValue(answer.WeightedScore);
            ws.Cell(answerRow, 4).SetValue(answer.Count);
            ws.Cell(answerRow, 5).SetFormulaA1($"=D{answerRow}/$B$5")
                .Style.NumberFormat.SetFormat("0.00%");
            ws.Cell(answerRow, 6).SetFormulaA1($"=C{answerRow}*E{answerRow}")
                .Style.NumberFormat.SetFormat("0.00");

            answerRow++;
        }
        row = answerRow;
    }

    private static void WriteMethodologyResult(this IXLWorksheet ws, string title, List<int> questionRows, int methodologyRow)
    {
        ws.Range(methodologyRow, 1, methodologyRow, 6)
            .Style
            .Font.SetBold()
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
            .Alignment.SetWrapText()
            .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            .Border.SetOutsideBorderColor(XLColor.FromHtml("#004C70"));

        ws.Cell(methodologyRow, 1).SetValue(title)
            .Style
            .NumberFormat.SetFormat(ExportHelper.AccountingFormat)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

        ws.Row(methodologyRow).Height = 30;
        ws.Range(methodologyRow, 1, methodologyRow, 2).Merge();

        var weightedScore = "=SUM(";
        var result = "=(";

        foreach (var row in questionRows)
        {
            var lastChar = weightedScore[^1];
            if (lastChar == '(') weightedScore += $"C{row}";
            else weightedScore += $",C{row}";

            var averageWeight = $"(C{row}/C{methodologyRow}*F{row})";
            if (lastChar == '(') result += averageWeight;
            else result += $"+{averageWeight}";
        }

        weightedScore += ")";
        result += $")/C{methodologyRow}*$B$5";

        ws.Cell(methodologyRow, 3).SetFormulaA1(weightedScore)
            .Style
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(methodologyRow, 6).SetFormulaA1(result)
            .Style
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.FromHtml("#42789F"))
            .NumberFormat.SetFormat("0.00")
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
    }

    public static void ConcludeReport(this IXLWorksheet ws)
    {
        ws.Column(1).Width = 13;
        ws.Column(2).Width = 42;
    }
}