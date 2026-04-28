using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace clavideDor_blz.Services;

/// <summary>
/// Service responsible for exporting score reports as downloadable PDF files.
/// </summary>
public class PdfExportService
{
    private readonly ILogger<PdfExportService> _logger;

    public PdfExportService(ILogger<PdfExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Export a score report PDF and return file metadata + content bytes.
    /// </summary>
    public Task<PdfExportResult> ExportScoreReportAsync(GameSessionStatistics statistics)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));

        var safePlayerName = SanitizeFileName(statistics.PlayerName);
        var fileName = $"clavier-dor-score-{safePlayerName}-{statistics.GameSessionId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        var categories = statistics.CategoriesCompleted.Count == 0
            ? "None"
            : string.Join(", ", statistics.CategoriesCompleted);

        var accuracy = statistics.AccuracyPercentage;
        var scoreAnalysis = GetScoreAnalysis(statistics.TotalScore);
        var accuracyAnalysis = GetAccuracyAnalysis(accuracy);
        var duration = statistics.EndDate - statistics.StartDate;

        var documentBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header().Column(column =>
                {
                    column.Item().Background(Colors.Blue.Darken2).Padding(18).Column(header =>
                    {
                        header.Item().Text("Clavier d’Or - Score Analysis")
                            .Bold()
                            .FontSize(24)
                            .FontColor(Colors.White);
                        header.Item().Text($"Session #{statistics.GameSessionId} • {statistics.EndDate:yyyy-MM-dd HH:mm}")
                            .FontColor(Colors.Blue.Lighten4);
                    });
                });

                page.Content().Column(column =>
                {
                    column.Spacing(14);

                    column.Item().PaddingTop(12).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(12).Column(details =>
                    {
                        details.Item().Text("Player Summary").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                        details.Item().Text($"Player: {statistics.PlayerName}");
                        details.Item().Text($"Role: {GetRoleLabel(statistics.PlayerRole)}");
                        details.Item().Text($"Game duration: {(int)duration.TotalMinutes}m {duration.Seconds}s");
                    });

                    column.Item().Row(row =>
                    {
                        row.Spacing(10);

                        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(card =>
                        {
                            card.Item().Text("Final Score").Bold();
                            card.Item().Text(statistics.TotalScore.ToString()).FontSize(20).Bold().FontColor(Colors.Green.Darken3);
                            card.Item().Text(scoreAnalysis).FontSize(10);
                        });

                        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(card =>
                        {
                            card.Item().Text("Accuracy").Bold();
                            card.Item().Text($"{accuracy:F1}%").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            card.Item().Text(accuracyAnalysis).FontSize(10);
                        });
                    });

                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(12).Column(metrics =>
                    {
                        metrics.Spacing(6);
                        metrics.Item().Text("Detailed Metrics").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                        metrics.Item().Text($"Answered questions: {statistics.TotalQuestionsAnswered}");
                        metrics.Item().Text($"Correct answers: {statistics.CorrectAnswers}");
                        metrics.Item().Text($"Wrong answers: {statistics.IncorrectAnswers}");
                        metrics.Item().Text($"Categories completed: {categories}");
                    });

                    column.Item().BorderLeft(4).BorderColor(Colors.Orange.Medium).Background(Colors.Orange.Lighten5).Padding(10).Column(analysis =>
                    {
                        analysis.Spacing(4);
                        analysis.Item().Text("Analysis Notes").Bold().FontColor(Colors.Orange.Darken3);
                        analysis.Item().Text($"• Score review: {scoreAnalysis}");
                        analysis.Item().Text($"• Accuracy review: {accuracyAnalysis}");
                        analysis.Item().Text("• Focus recommendation: prioritize categories with past wrong answers and replay boss questions.");
                    });
                });

                page.Footer().AlignCenter().Text("Clavier d’Or • Generated by QuestPDF").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();

        _logger.LogInformation("Score PDF generated for download as {FileName} for session {GameSessionId}", fileName, statistics.GameSessionId);

        return Task.FromResult(new PdfExportResult(fileName, "application/pdf", documentBytes));
    }

    private static string GetRoleLabel(Models.PlayerRole role)
    {
        return role switch
        {
            Models.PlayerRole.FrontDeveloper => "Front Developer",
            Models.PlayerRole.BackDeveloper => "Back Developer",
            Models.PlayerRole.MobileDeveloper => "Mobile Developer",
            _ => "Unknown"
        };
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(invalidCharacters.Contains(character) ? '_' : character);
        }

        return string.IsNullOrWhiteSpace(builder.ToString()) ? "player" : builder.ToString();
    }

    private static string GetScoreAnalysis(int score)
    {
        return score switch
        {
            >= 1200 => "Outstanding performance with strong consistency.",
            >= 800 => "Very good run with solid mastery.",
            >= 400 => "Good progress; a few categories need reinforcement.",
            _ => "Early-stage performance; more practice recommended."
        };
    }

    private static string GetAccuracyAnalysis(double accuracy)
    {
        return accuracy switch
        {
            >= 90 => "Excellent precision and decision making.",
            >= 75 => "Great accuracy with minor mistakes.",
            >= 50 => "Average accuracy; review weak topics.",
            _ => "Low accuracy; revisit fundamentals and retry."
        };
    }
}

public record PdfExportResult(string FileName, string ContentType, byte[] Content);
