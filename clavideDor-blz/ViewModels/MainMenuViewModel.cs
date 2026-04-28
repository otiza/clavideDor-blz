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

            // TODO: Check if there are any unfinished games
            // For now, we assume there might be one
            HasUnfinishedGame = false;
            ResumeGameButtonText = "Resume Game";

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
            // Navigation will be handled by the page
            // Page will navigate to /newgame
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

            // TODO: Load and resume the unfinished game
            // Navigation will be handled by the page
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
            // Page will navigate to /history
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

