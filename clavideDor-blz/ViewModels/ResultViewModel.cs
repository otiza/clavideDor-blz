using clavideDor_blz.Services;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// ViewModel for the result page
/// Shows final score and statistics for a completed game
/// </summary>
public class ResultViewModel : BaseViewModel
{
    private readonly ScoreService _scoreService;
    private readonly PdfExportService _pdfExportService;
    private readonly ILogger<ResultViewModel> _logger;

    private int _gameSessionId;
    private GameSessionStatistics? _statistics;
    private bool _pdfExporting;
    private string _lastExportedFilePath = string.Empty;

    public int GameSessionId
    {
        get => _gameSessionId;
        set => SetProperty(ref _gameSessionId, value);
    }

    public GameSessionStatistics? Statistics
    {
        get => _statistics;
        set => SetProperty(ref _statistics, value);
    }

    public bool PdfExporting
    {
        get => _pdfExporting;
        set => SetProperty(ref _pdfExporting, value);
    }

    public string LastExportedFilePath
    {
        get => _lastExportedFilePath;
        set => SetProperty(ref _lastExportedFilePath, value);
    }

    public ResultViewModel(ScoreService scoreService, PdfExportService pdfExportService, ILogger<ResultViewModel> logger)
    {
        _scoreService = scoreService;
        _pdfExportService = pdfExportService;
        _logger = logger;
    }

    /// <summary>
    /// Load game statistics
    /// </summary>
    public async Task<bool> LoadResultsAsync(int gameSessionId)
    {
        try
        {
            IsLoading = true;
            ClearError();

            GameSessionId = gameSessionId;

            // Load statistics
            Statistics = await _scoreService.GetGameSessionStatisticsAsync(gameSessionId);

            _logger.LogInformation($"Loaded results for game session {gameSessionId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading results for session {gameSessionId}");
            SetError("An error occurred while loading results");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Export results to PDF
    /// </summary>
    public async Task<bool> ExportPdfAsync()
    {
        try
        {
            if (Statistics == null)
            {
                SetError("No statistics to export");
                return false;
            }

            PdfExporting = true;
            ClearError();
            LastExportedFilePath = string.Empty;

            _logger.LogInformation($"Exporting PDF for game session {GameSessionId}");
            LastExportedFilePath = await _pdfExportService.ExportScoreReportAsync(Statistics);

            _logger.LogInformation("PDF exported successfully to {FilePath}", LastExportedFilePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF");
            SetError("An error occurred while exporting PDF");
            return false;
        }
        finally
        {
            PdfExporting = false;
        }
    }

    /// <summary>
    /// Get score rating
    /// </summary>
    public string GetScoreRating()
    {
        if (Statistics == null)
            return "N/A";

        return Statistics.TotalScore switch
        {
            >= 1500 => "Outstanding! 🏆",
            >= 1000 => "Excellent! ⭐",
            >= 500 => "Good! 👍",
            >= 100 => "Fair 👌",
            _ => "Keep trying! 💪"
        };
    }

    /// <summary>
    /// Get accuracy rating
    /// </summary>
    public string GetAccuracyRating()
    {
        if (Statistics == null)
            return "N/A";

        return Statistics.AccuracyPercentage switch
        {
            >= 90 => "Perfect! 🎯",
            >= 75 => "Great! 👏",
            >= 50 => "Good 👍",
            _ => "Keep practicing! 📚"
        };
    }
}
