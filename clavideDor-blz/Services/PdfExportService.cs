using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace clavideDor_blz.Services;

/// <summary>
/// Service responsible for exporting score reports as PDF files.
/// </summary>
public class PdfExportService
{
    private readonly ILogger<PdfExportService> _logger;

    public PdfExportService(ILogger<PdfExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Export a score report PDF and return the generated file path.
    /// </summary>
    public Task<string> ExportScoreReportAsync(GameSessionStatistics statistics)
    {
        if (statistics == null)
            throw new ArgumentNullException(nameof(statistics));

        var exportsDirectory = Path.Combine(AppContext.BaseDirectory, "exports");
        Directory.CreateDirectory(exportsDirectory);

        var safePlayerName = SanitizeFileName(statistics.PlayerName);
        var fileName = $"clavier-dor-score-{safePlayerName}-{statistics.GameSessionId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var filePath = Path.Combine(exportsDirectory, fileName);

        var categories = statistics.CategoriesCompleted.Count == 0
            ? "None"
            : string.Join(", ", statistics.CategoriesCompleted);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text("Clavier d’Or - Score")
                        .Bold()
                        .FontSize(22)
                        .FontColor(Colors.Blue.Medium);

                    column.Item().Text($"Player: {statistics.PlayerName}");
                    column.Item().Text($"Role: {GetRoleLabel(statistics.PlayerRole)}");
                    column.Item().Text($"Final score: {statistics.TotalScore}");
                    column.Item().Text($"Date: {statistics.EndDate:yyyy-MM-dd HH:mm}");
                    column.Item().Text($"Answered questions: {statistics.TotalQuestionsAnswered}");
                    column.Item().Text($"Categories completed: {categories}");
                });
            });
        }).GeneratePdf(filePath);

        _logger.LogInformation("Score PDF exported to {FilePath} for session {GameSessionId}", filePath, statistics.GameSessionId);

        return Task.FromResult(filePath);
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
}
