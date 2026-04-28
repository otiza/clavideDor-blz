using clavideDor_blz.Models;
using clavideDor_blz.Services;

namespace clavideDor_blz.ViewModels;

/// <summary>
/// ViewModel for the game page
/// Handles game logic, question display, answers, and jokers
/// </summary>
public class GameViewModel : BaseViewModel
{
    private readonly GameService _gameService;
    private readonly ScoreService _scoreService;
    private readonly ILogger<GameViewModel> _logger;

    private int _gameSessionId;
    private GameSession? _currentSession;
    private Question? _currentQuestion;
    private string _selectedAnswer = string.Empty;
    private bool _answered;
    private bool _answerCorrect;
    private int _currentQuestionIndex;
    private int _totalQuestionsAnswered;
    private bool _showHint;
    private bool _showRetryOption;
    private List<string> _hiddenWrongAnswers = [];
    private string _hintText = string.Empty;

    public int GameSessionId
    {
        get => _gameSessionId;
        set => SetProperty(ref _gameSessionId, value);
    }

    public GameSession? CurrentSession
    {
        get => _currentSession;
        set => SetProperty(ref _currentSession, value);
    }

    public Question? CurrentQuestion
    {
        get => _currentQuestion;
        set => SetProperty(ref _currentQuestion, value);
    }

    public string SelectedAnswer
    {
        get => _selectedAnswer;
        set => SetProperty(ref _selectedAnswer, value);
    }

    public bool Answered
    {
        get => _answered;
        set => SetProperty(ref _answered, value);
    }

    public bool AnswerCorrect
    {
        get => _answerCorrect;
        set => SetProperty(ref _answerCorrect, value);
    }

    public int CurrentQuestionIndex
    {
        get => _currentQuestionIndex;
        set => SetProperty(ref _currentQuestionIndex, value);
    }

    public int TotalQuestionsAnswered
    {
        get => _totalQuestionsAnswered;
        set => SetProperty(ref _totalQuestionsAnswered, value);
    }

    public bool ShowHint
    {
        get => _showHint;
        set => SetProperty(ref _showHint, value);
    }

    public bool ShowRetryOption
    {
        get => _showRetryOption;
        set => SetProperty(ref _showRetryOption, value);
    }

    public List<string> HiddenWrongAnswers
    {
        get => _hiddenWrongAnswers;
        set => SetProperty(ref _hiddenWrongAnswers, value);
    }

    public string HintText
    {
        get => _hintText;
        set => SetProperty(ref _hintText, value);
    }

    public GameViewModel(GameService gameService, ScoreService scoreService, ILogger<GameViewModel> logger)
    {
        _gameService = gameService;
        _scoreService = scoreService;
        _logger = logger;
    }

    /// <summary>
    /// Initialize the game session
    /// </summary>
    public async Task<bool> InitializeAsync(int gameSessionId)
    {
        try
        {
            IsLoading = true;
            ClearError();

            GameSessionId = gameSessionId;

            // Load game session
            CurrentSession = await _gameService.GetGameSessionAsync(gameSessionId);
            if (CurrentSession == null)
            {
                SetError("Game session not found");
                return false;
            }

            if (CurrentSession.IsFinished)
            {
                SetError("This game session is already finished");
                return false;
            }

            TotalQuestionsAnswered = CurrentSession.AnsweredQuestions.Count;

            // Load next question
            await LoadNextQuestionAsync();

            _logger.LogInformation($"Game session {gameSessionId} initialized");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error initializing game session {gameSessionId}");
            SetError("An error occurred while initializing the game");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load the next unanswered question
    /// </summary>
    private async Task LoadNextQuestionAsync()
    {
        try
        {
            CurrentQuestion = await _gameService.GetNextQuestionAsync(GameSessionId);

            if (CurrentQuestion == null)
            {
                // No remaining question means the game reached the end.
                // Keep error empty so the UI can show the "Game Finished" panel.
                ClearError();
                return;
            }

            CurrentQuestionIndex = TotalQuestionsAnswered + 1;
            SelectedAnswer = string.Empty;
            Answered = false;
            AnswerCorrect = false;
            ShowHint = false;
            ShowRetryOption = false;
            HintText = string.Empty;
            HiddenWrongAnswers.Clear();

            _logger.LogInformation($"Loaded question {CurrentQuestion.Id}: {CurrentQuestion.Text}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading next question");
            SetError("An error occurred while loading the next question");
        }
    }

    /// <summary>
    /// Submit the selected answer
    /// </summary>
    public async Task<bool> SubmitAnswerAsync()
    {
        try
        {
            if (CurrentQuestion == null)
            {
                SetError("No question loaded");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedAnswer))
            {
                SetError("Please select an answer");
                return false;
            }

            IsLoading = true;
            ClearError();

            // Submit answer
            var answeredQuestion = await _gameService.SubmitAnswerAsync(GameSessionId, CurrentQuestion.Id, SelectedAnswer);

            Answered = true;
            AnswerCorrect = answeredQuestion.IsCorrect;

            // Reload session to get updated score
            CurrentSession = await _gameService.GetGameSessionAsync(GameSessionId);
            TotalQuestionsAnswered++;

            _logger.LogInformation($"Answer submitted: {SelectedAnswer}, Correct: {AnswerCorrect}");

            // Handle Back Developer joker (automatic retry on wrong answer)
            if (!AnswerCorrect && CurrentSession?.Player?.Role == PlayerRole.BackDeveloper && !CurrentSession.BackDeveloperJokerUsed)
            {
                ShowRetryOption = true;
                _logger.LogInformation("Back Developer can retry this question");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer");
            SetError("An error occurred while submitting your answer");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Move to the next question
    /// </summary>
    public async Task<bool> NextQuestionAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            await LoadNextQuestionAsync();

            if (CurrentQuestion == null)
            {
                // Game finished
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving to next question");
            SetError("An error occurred");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Use joker - depends on role
    /// </summary>
    public async Task<bool> UseJokerAsync()
    {
        try
        {
            if (CurrentSession?.Player == null)
            {
                SetError("Player not found");
                return false;
            }

            IsLoading = true;
            ClearError();

            var jokerUsed = await _gameService.UseJokerAsync(GameSessionId, CurrentSession.Player.Role);

            if (!jokerUsed)
            {
                SetError("You have already used this joker");
                return false;
            }

            switch (CurrentSession.Player.Role)
            {
                case PlayerRole.FrontDeveloper:
                    // Skip current question without penalty
                    await SkipQuestionAsync();
                    break;

                case PlayerRole.BackDeveloper:
                    // Mark joker as used - will enable auto-retry after wrong answer
                    _logger.LogInformation("Back Developer joker activated - auto-retry enabled");
                    break;

                case PlayerRole.MobileDeveloper:
                    // Show hint
                    await ShowHintAsync();
                    break;
            }

            _logger.LogInformation($"Joker used for {CurrentSession.Player.Role}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using joker");
            SetError("An error occurred while using the joker");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Skip current question (Front Developer joker)
    /// </summary>
    private async Task SkipQuestionAsync()
    {
        try
        {
            // Just load next question without penalty
            await LoadNextQuestionAsync();
            _logger.LogInformation("Question skipped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping question");
            SetError("An error occurred while skipping the question");
        }
    }

    /// <summary>
    /// Show hint (Mobile Developer joker)
    /// </summary>
    private async Task ShowHintAsync()
    {
        try
        {
            if (CurrentQuestion == null)
                return;

            // For now, provide a generic hint by highlighting correct answer
            // Hide two wrong answers
            var wrongAnswers = new List<(string answer, char letter)>
            {
                ("A", 'A'),
                ("B", 'B'),
                ("C", 'C'),
                ("D", 'D')
            }.Where(x => !x.letter.ToString().Equals(CurrentQuestion.Correct, StringComparison.OrdinalIgnoreCase))
             .Select(x => x.letter.ToString())
             .Take(2)
             .ToList();

            HiddenWrongAnswers = wrongAnswers;
            HintText = "Two wrong answers have been eliminated!";
            ShowHint = true;

            _logger.LogInformation("Hint shown - wrong answers eliminated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing hint");
            SetError("An error occurred while showing the hint");
        }
    }

    /// <summary>
    /// Retry the question (Back Developer auto-retry)
    /// </summary>
    public async Task<bool> RetryQuestionAsync()
    {
        try
        {
            if (CurrentQuestion == null)
            {
                SetError("No question to retry");
                return false;
            }

            IsLoading = true;
            ClearError();

            // Reset answer
            SelectedAnswer = string.Empty;
            Answered = false;
            AnswerCorrect = false;
            ShowRetryOption = false;

            _logger.LogInformation("Question retried");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying question");
            SetError("An error occurred while retrying");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Save game progress
    /// </summary>
    public async Task<bool> SaveProgressAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            await _gameService.SaveProgressAsync(GameSessionId);

            _logger.LogInformation($"Game session {GameSessionId} progress saved");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving progress");
            SetError("An error occurred while saving progress");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Finish the game
    /// </summary>
    public async Task<bool> FinishGameAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            await _gameService.FinishGameAsync(GameSessionId);

            _logger.LogInformation($"Game session {GameSessionId} finished with score {CurrentSession?.Score}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finishing game");
            SetError("An error occurred while finishing the game");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Check if a joker is available
    /// </summary>
    public bool IsJokerAvailable()
    {
        if (CurrentSession?.Player == null)
            return false;

        return CurrentSession.Player.Role switch
        {
            PlayerRole.FrontDeveloper => !CurrentSession.FrontDeveloperJokerUsed,
            PlayerRole.BackDeveloper => !CurrentSession.BackDeveloperJokerUsed,
            PlayerRole.MobileDeveloper => !CurrentSession.MobileDeveloperJokerUsed,
            _ => false
        };
    }

    /// <summary>
    /// Get joker button text based on role
    /// </summary>
    public string GetJokerButtonText()
    {
        if (CurrentSession?.Player == null)
            return "Joker";

        return CurrentSession.Player.Role switch
        {
            PlayerRole.FrontDeveloper => "🔄 Skip Question",
            PlayerRole.BackDeveloper => "🔁 Auto-Retry",
            PlayerRole.MobileDeveloper => "💡 Hint",
            _ => "Joker"
        };
    }

    /// <summary>
    /// Get progress percentage
    /// </summary>
    public int GetProgressPercentage()
    {
        // Assuming approximately 100 questions total
        const int totalQuestions = 100;
        return Math.Min((TotalQuestionsAnswered * 100) / totalQuestions, 100);
    }
}
