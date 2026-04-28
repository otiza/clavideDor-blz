using clavideDor_blz.Services;
using clavideDor_blz.Models;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// ViewModel for the main menu page
/// Handles navigation and game start/resume logic
/// </summary>
public class MainMenuViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly ILogger<MainMenuViewModel> _logger;
    private bool _hasUnfinishedGame;
    private string _resumeGameButtonText = "Resume Game";
    private int _resumeGameSessionId;
    private List<GameSession> _unfinishedGames = [];
    private int _selectedResumeSessionId;

    public bool HasUnfinishedGame
    {
        get => _hasUnfinishedGame;
        set => SetProperty(ref _hasUnfinishedGame, value);
    }

    public string ResumeGameButtonText
    {
        get => _resumeGameButtonText;
        set => SetProperty(ref _resumeGameButtonText, value);
    }

    public int ResumeGameSessionId
    {
        get => _resumeGameSessionId;
        set => SetProperty(ref _resumeGameSessionId, value);
    }

    public List<GameSession> UnfinishedGames
    {
        get => _unfinishedGames;
        set => SetProperty(ref _unfinishedGames, value);
    }

    public int SelectedResumeSessionId
    {
        get => _selectedResumeSessionId;
        set => SetProperty(ref _selectedResumeSessionId, value);
    }

    public MainMenuViewModel(GameService gameService, ILogger<MainMenuViewModel> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize the view model (check for unfinished games)
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            UnfinishedGames = await _gameService.GetUnfinishedGamesAsync();
            HasUnfinishedGame = UnfinishedGames.Count > 0;

            if (HasUnfinishedGame)
            {
                SelectedResumeSessionId = UnfinishedGames[0].Id;
                ResumeGameSessionId = SelectedResumeSessionId;
                ResumeGameButtonText = "Resume Selected Game";
            }
            else
            {
                SelectedResumeSessionId = 0;
                ResumeGameSessionId = 0;
                ResumeGameButtonText = "Resume Game";
            }

            _logger.LogInformation("Main menu initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing main menu");
            SetError("An error occurred while initializing the menu");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle New Game button click
    /// </summary>
    public async Task OnNewGameAsync()
    {
        try
        {
            ClearError();
            _logger.LogInformation("New Game requested");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNewGame");
            SetError("An error occurred");
        }
    }

    /// <summary>
    /// Handle Resume Game button click
    /// </summary>
    public async Task OnResumeGameAsync()
    {
        try
        {
            ClearError();
            _logger.LogInformation("Resume Game requested");

            if (!HasUnfinishedGame)
            {
                SetError("No unfinished game found");
                return;
            }

            if (SelectedResumeSessionId <= 0)
            {
                SetError("Please select a game to resume");
                return;
            }

            ResumeGameSessionId = SelectedResumeSessionId;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnResumeGame");
            SetError("An error occurred");
        }
    }

    /// <summary>
    /// Handle History button click
    /// </summary>
    public async Task OnHistoryAsync()
    {
        try
        {
            ClearError();
            _logger.LogInformation("History requested");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnHistory");
            SetError("An error occurred");
        }
    }

    public string GetResumeOptionLabel(GameSession session)
    {
        var playerName = session.Player?.Name ?? "Unknown";
        var startedAt = session.StartedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        var answered = session.AnsweredQuestions.Count;
        return $"{playerName} • Started {startedAt} • Score {session.Score} • {answered} answered";
    }

    /// <summary>
    /// Handle Quit button click
    /// </summary>
    public void OnQuit()
    {
        try
        {
            _logger.LogInformation("Quit requested");
            // Application will close
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnQuit");
        }
    }
}
