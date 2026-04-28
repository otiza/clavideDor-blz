using clavideDor_blz.Services;

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

            var unfinishedGame = await _gameService.GetLatestUnfinishedGameAsync();
            HasUnfinishedGame = unfinishedGame != null;
            ResumeGameSessionId = unfinishedGame?.Id ?? 0;
            ResumeGameButtonText = unfinishedGame == null
                ? "Resume Game"
                : $"Resume: {unfinishedGame.Player?.Name ?? "Player"}";

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
