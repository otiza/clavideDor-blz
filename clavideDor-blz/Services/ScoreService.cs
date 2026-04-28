using clavideDor_blz.Data;
using clavideDor_blz.Models;
using Microsoft.EntityFrameworkCore;

namespace clavideDor_blz.Services;

/// <summary>
/// Service for scoring logic
/// </summary>
public class ScoreService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ScoreService> _logger;

    // Scoring constants
    private const int CorrectNormalQuestionPoints = 10;
    private const int CorrectBossQuestionPoints = 15;
    private const int IncorrectQuestionPoints = 0;

    public ScoreService(AppDbContext context, ILogger<ScoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calculate points for an answered question
    /// </summary>
    public int CalculatePoints(Question question, bool isCorrect)
    {
        if (!isCorrect)
            return IncorrectQuestionPoints;

        return question.IsBoss ? CorrectBossQuestionPoints : CorrectNormalQuestionPoints;
    }

    /// <summary>
    /// Get the final score for a completed game session
    /// </summary>
    public async Task<int> GetGameSessionScoreAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(g => g.AnsweredQuestions)
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            return session.Score;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting score for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get statistics for a completed game session
    /// </summary>
    public async Task<GameSessionStatistics> GetGameSessionStatisticsAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(g => g.AnsweredQuestions)
                    .ThenInclude(aq => aq.Question)
                        .ThenInclude(q => q.Category)
                .Include(g => g.Player)
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            var stats = new GameSessionStatistics
            {
                GameSessionId = gameSessionId,
                PlayerName = session.Player?.Name ?? "Unknown",
                PlayerRole = session.Player?.Role ?? PlayerRole.FrontDeveloper,
                TotalScore = session.Score,
                TotalQuestionsAnswered = session.AnsweredQuestions.Count,
                CorrectAnswers = session.AnsweredQuestions.Count(aq => aq.IsCorrect),
                IncorrectAnswers = session.AnsweredQuestions.Count(aq => !aq.IsCorrect),
                StartDate = session.StartedAt,
                EndDate = session.FinishedAt ?? DateTime.UtcNow,
                CategoriesCompleted = session.AnsweredQuestions
                    .Select(aq => aq.Question?.Category?.Name)
                    .Where(c => c != null)
                    .Cast<string>()
                    .Distinct()
                    .ToList()
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting statistics for session {gameSessionId}");
            throw;
        }
    }

    /// <summary>
    /// Get accuracy percentage for a game session
    /// </summary>
    public async Task<double> GetAccuracyPercentageAsync(int gameSessionId)
    {
        try
        {
            var session = await _context.GameSessions
                .Include(g => g.AnsweredQuestions)
                .FirstOrDefaultAsync(g => g.Id == gameSessionId);

            if (session == null)
                throw new ArgumentException($"Game session {gameSessionId} not found");

            var totalAnswered = session.AnsweredQuestions.Count;
            if (totalAnswered == 0)
                return 0;

            var correctCount = session.AnsweredQuestions.Count(aq => aq.IsCorrect);
            return (double)correctCount / totalAnswered * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calculating accuracy for session {gameSessionId}");
            throw;
        }
    }
}

/// <summary>
/// Data class for game session statistics
/// </summary>
public class GameSessionStatistics
{
    public int GameSessionId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public PlayerRole PlayerRole { get; set; }
    public int TotalScore { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> CategoriesCompleted { get; set; } = [];

    public double AccuracyPercentage => 
        TotalQuestionsAnswered == 0 ? 0 : (double)CorrectAnswers / TotalQuestionsAnswered * 100;
}

