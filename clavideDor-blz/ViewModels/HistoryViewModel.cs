using clavideDor_blz.Models;
using clavideDor_blz.Services;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// ViewModel for the history page
/// Shows all finished games for a player
/// </summary>
public class HistoryViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly ILogger<HistoryViewModel> _logger;

    private List<GameSession> _allGameSessions = [];
    private List<GameSession> _filteredSessions = [];
    private string _filterText = string.Empty;

    public List<GameSession> AllGameSessions
    {
        get => _allGameSessions;
        set => SetProperty(ref _allGameSessions, value);
    }

    public List<GameSession> FilteredSessions
    {
        get => _filteredSessions;
        set => SetProperty(ref _filteredSessions, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                ApplyFilter();
            }
        }
    }

    public HistoryViewModel(GameService gameService, ILogger<HistoryViewModel> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Load all finished games (for now, all sessions)
    /// In a real app, we'd load all finished games from all players
    /// </summary>
    public async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            // TODO: Load all finished games from database
            // For now, we'll show an empty list as a placeholder
            AllGameSessions = [];
            ApplyFilter();

            _logger.LogInformation("Game history loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game history");
            SetError("An error occurred while loading history");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Apply filter to game sessions
    /// </summary>
    private void ApplyFilter()
    {
        if (string.IsNullOrEmpty(FilterText))
        {
            FilteredSessions = AllGameSessions.ToList();
        }
        else
        {
            FilteredSessions = AllGameSessions
                .Where(s => s.Player?.Name != null && s.Player.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    /// <summary>
    /// Get role display name
    /// </summary>
    public string GetRoleDisplayName(PlayerRole? role)
    {
        return role switch
        {
            PlayerRole.FrontDeveloper => "Front Developer",
            PlayerRole.BackDeveloper => "Back Developer",
            PlayerRole.MobileDeveloper => "Mobile Developer",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get formatted date
    /// </summary>
    public string FormatDate(DateTime? date)
    {
        return date?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
    }
}

