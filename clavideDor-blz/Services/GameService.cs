using clavideDor_blz.Data;
using clavideDor_blz.Models;
using Microsoft.EntityFrameworkCore;

namespace clavideDor_blz.Services;

/// <summary>
/// Service for managing game sessions and gameplay logic
/// </summary>
public class GameService
{
    private readonly AppDbContext _context;
    private readonly ScoreService _scoreService;
    private readonly ILogger<GameService> _logger;

    public GameService(AppDbContext context, ScoreService scoreService, ILogger<GameService> logger)
    {
        _context = context;
        _scoreService = scoreService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new game session for a player
    /// </summary>
    public async Task<GameSession> StartNewGameAsync(string playerName, PlayerRole role)
    {
        try
        {
            _logger.LogInformation($"Starting new game for player {playerName} with role {role}");

            // Create or get the player
            var player = new Player
            {
                Name = playerName,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Create a new game session
            var session = new GameSession
            {
                PlayerId = player.Id,
                Score = 0,
                IsFinished = false,
                StartedAt = DateTime.UtcNow,
                FrontDeveloperJokerUsed = false,
                BackDeveloperJokerUsed = false,
                MobileDeveloperJokerUsed = false
            };

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Game session {session.Id} created for player {player.Name}");

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting new game for {playerName}");
            throw;
        }
    }

    /// <summary>
    /// Get the next unanswered question for a game session
    /// </summary>
    public async Task<Question?> GetNextQuestionAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(g => g.AnsweredQuestions)
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            if (session.IsFinished)
                return null;

            // Get IDs of already answered questions
            var answeredQuestionIds = session.AnsweredQuestions
                .Select(aq => aq.QuestionId)
                .ToList();

            // Get next unanswered question
            var nextQuestion = await _context.Questions
                .Where(q => !answeredQuestionIds.Contains(q.Id))
                .FirstOrDefaultAsync();

            return nextQuestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting next question for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Submit an answer to a question
    /// </summary>
    public async Task<AnsweredQuestion> SubmitAnswerAsync(int gameSessionId, int questionId, string selectedAnswer)
    {
        try
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            if (session.IsFinished)
                throw new InvalidOperationException("Game session is already finished");

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                throw new ArgumentException($"Question {questionId} not found");

            // Check if already answered
            var existingAnswer = await _context.AnsweredQuestions
                .FirstOrDefaultAsync(aq => aq.GameSessionId == gameSessionId && aq.QuestionId == questionId);

            if (existingAnswer != null)
                throw new InvalidOperationException("Question already answered in this session");

            // Check answer correctness
            var isCorrect = selectedAnswer.Equals(question.Correct, StringComparison.OrdinalIgnoreCase);
            var pointsEarned = _scoreService.CalculatePoints(question, isCorrect);

            // Create answered question record
            var answeredQuestion = new AnsweredQuestion
            {
                GameSessionId = gameSessionId,
                QuestionId = questionId,
                SelectedAnswer = selectedAnswer,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                AnsweredAt = DateTime.UtcNow
            };

            _context.AnsweredQuestions.Add(answeredQuestion);

            // Update session score
            session.Score += pointsEarned;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Answer submitted for session {gameSessionId}, question {questionId}. Correct: {isCorrect}, Points: {pointsEarned}");

            return answeredQuestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error submitting answer for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Save the current game progress
    /// </summary>
    public async Task SaveProgressAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            // Just save the context (session state is automatically tracked)
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Game session {gameSessionId} progress saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving progress for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Mark a game session as finished
    /// </summary>
    public async Task FinishGameAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            session.IsFinished = true;
            session.FinishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Game session {gameSessionId} marked as finished with score {session.Score}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error finishing game session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get all answered questions for a game session
    /// </summary>
    public async Task<List<AnsweredQuestion>> GetAnsweredQuestionsAsync(int gameSessionId)
    {
        try
        {
            var answeredQuestions = await _context.AnsweredQuestions
                .Where(aq => aq.GameSessionId == gameSessionId)
                .Include(aq => aq.Question!)
                    .ThenInclude(q => q.Category)
                .OrderBy(aq => aq.AnsweredAt)
                .ToListAsync();

            return answeredQuestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting answered questions for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get a game session with all related data
    /// </summary>
    public async Task<GameSession?> GetGameSessionAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(g => g.Player)
                .Include(g => g.AnsweredQuestions)
                    .ThenInclude(aq => aq.Question)
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting game session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get all finished game sessions with player and answered-question data
    /// </summary>
    public async Task<List<GameSession>> GetFinishedGamesAsync()
    {
        try
        {
            var finishedGames = await _context.GameSessions
                .AsNoTracking()
                .Where(g => g.IsFinished)
                .Include(g => g.Player)
                .Include(g => g.AnsweredQuestions)
                .OrderByDescending(g => g.FinishedAt)
                .ThenByDescending(g => g.StartedAt)
                .ToListAsync();

            return finishedGames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading finished game sessions");
            throw;
        }
    }

    /// <summary>
    /// Resume an unfinished game for a player
    /// </summary>
    public async Task<GameSession?> GetUnfinishedGameAsync(int playerId)
    {
        try
        {
            var unfinishedGame = await _context.GameSessions
                .Include(g => g.Player)
                .Include(g => g.AnsweredQuestions)
                .Where(g => g.PlayerId == playerId && !g.IsFinished)
                .OrderByDescending(g => g.StartedAt)
                .FirstOrDefaultAsync();

            return unfinishedGame;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting unfinished game for player {playerId}");
            throw;
        }
    }

    /// <summary>
    /// Get the most recent unfinished game session across all players.
    /// </summary>
    public async Task<GameSession?> GetLatestUnfinishedGameAsync()
    {
        try
        {
            var unfinishedGame = await _context.GameSessions
                .Include(g => g.Player)
                .Include(g => g.AnsweredQuestions)
                .Where(g => !g.IsFinished)
                .OrderByDescending(g => g.StartedAt)
                .FirstOrDefaultAsync();

            return unfinishedGame;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest unfinished game");
            throw;
        }
    }

    /// <summary>
    /// Use a joker (role-specific ability)
    /// </summary>
    public async Task<bool> UseJokerAsync(int gameSessionId, PlayerRole role)
    {
        try
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            bool jokerUsed = false;

            switch (role)
            {
                case PlayerRole.FrontDeveloper:
                    if (!session.FrontDeveloperJokerUsed)
                    {
                        session.FrontDeveloperJokerUsed = true;
                        jokerUsed = true;
                    }
                    break;

                case PlayerRole.BackDeveloper:
                    if (!session.BackDeveloperJokerUsed)
                    {
                        session.BackDeveloperJokerUsed = true;
                        jokerUsed = true;
                    }
                    break;

                case PlayerRole.MobileDeveloper:
                    if (!session.MobileDeveloperJokerUsed)
                    {
                        session.MobileDeveloperJokerUsed = true;
                        jokerUsed = true;
                    }
                    break;
            }

            if (jokerUsed)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Joker used for {role} in session {gameSessionId}");
            }

            return jokerUsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error using joker for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Check if a joker has been used
    /// </summary>
    public async Task<bool> IsJokerUsedAsync(int gameSessionId, PlayerRole role)
    {
        try
        {
            var session = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            return role switch
            {
                PlayerRole.FrontDeveloper => session.FrontDeveloperJokerUsed,
                PlayerRole.BackDeveloper => session.BackDeveloperJokerUsed,
                PlayerRole.MobileDeveloper => session.MobileDeveloperJokerUsed,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking joker status for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get all finished games for a player
    /// </summary>
    public async Task<List<GameSession>> GetPlayerGameHistoryAsync(int playerId)
    {
        try
        {
            var gameHistory = await _context.GameSessions
                .Where(g => g.PlayerId == playerId && g.IsFinished)
                .Include(g => g.Player)
                .Include(g => g.AnsweredQuestions)
                .OrderByDescending(g => g.FinishedAt)
                .ToListAsync();

            return gameHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting game history for player {playerId}");
            throw;
        }
    }
}
