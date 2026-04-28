using clavideDor_blz.Models;
using clavideDor_blz.Services;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// ViewModel for creating a new game
/// Handles player name input and role selection
/// </summary>
public class NewGameViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly ILogger<NewGameViewModel> _logger;

    private string _playerName = string.Empty;
    private PlayerRole _selectedRole = PlayerRole.FrontDeveloper;
    private int _createdGameSessionId;
    private bool _gameStarted;

    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

    public PlayerRole SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public int CreatedGameSessionId
    {
        get => _createdGameSessionId;
        set => SetProperty(ref _createdGameSessionId, value);
    }

    public bool GameStarted
    {
        get => _gameStarted;
        set => SetProperty(ref _gameStarted, value);
    }

    // Available roles for display
    public List<PlayerRole> AvailableRoles { get; } = new()
    {
        PlayerRole.FrontDeveloper,
        PlayerRole.BackDeveloper,
        PlayerRole.MobileDeveloper
    };

    public NewGameViewModel(GameService gameService, ILogger<NewGameViewModel> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Validate player name and role, then start a new game
    /// </summary>
    public async Task<bool> StartNewGameAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            // Validate player name
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                SetError("Please enter your name");
                return false;
            }

            if (PlayerName.Length < 2)
            {
                SetError("Name must be at least 2 characters long");
                return false;
            }

            if (PlayerName.Length > 100)
            {
                SetError("Name must not exceed 100 characters");
                return false;
            }

            _logger.LogInformation($"Starting new game for player '{PlayerName}' with role {SelectedRole}");

            // Create new game session
            var gameSession = await _gameService.StartNewGameAsync(PlayerName.Trim(), SelectedRole);

            CreatedGameSessionId = gameSession.Id;
            GameStarted = true;

            _logger.LogInformation($"New game session {gameSession.Id} created successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting new game");
            SetError("An error occurred while starting the game");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Get a human-readable description of a role
    /// </summary>
    public string GetRoleDescription(PlayerRole role) => role switch
    {
        PlayerRole.FrontDeveloper => "Front Developer: Can skip one question without penalty",
        PlayerRole.BackDeveloper => "Back Developer: Gets one automatic retry after a wrong answer",
        PlayerRole.MobileDeveloper => "Mobile Developer: Can reveal one hint",
        _ => "Unknown Role"
    };

    /// <summary>
    /// Get a short name for a role
    /// </summary>
    public string GetRoleDisplayName(PlayerRole role) => role switch
    {
        PlayerRole.FrontDeveloper => "Front Developer",
        PlayerRole.BackDeveloper => "Back Developer",
        PlayerRole.MobileDeveloper => "Mobile Developer",
        _ => "Unknown"
    };

    /// <summary>
    /// Reset the form
    /// </summary>
    public void Reset()
    {
        PlayerName = string.Empty;
        SelectedRole = PlayerRole.FrontDeveloper;
        GameStarted = false;
        CreatedGameSessionId = 0;
        ClearError();
    }
}

