namespace clavideDor_blz.Models;

public class GameSession
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Player
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Current score in the game
    /// </summary>
    public int Score { get; set; } = 0;

    /// <summary>
    /// Is the game finished?
    /// </summary>
    public bool IsFinished { get; set; } = false;

    /// <summary>
    /// Date when the game session started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the game session finished (nullable)
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Has the Front Developer used their joker (change question)?
    /// </summary>
    public bool FrontDeveloperJokerUsed { get; set; } = false;

    /// <summary>
    /// Has the Back Developer used their automatic retry?
    /// </summary>
    public bool BackDeveloperJokerUsed { get; set; } = false;

    /// <summary>
    /// Has the Mobile Developer used their hint?
    /// </summary>
    public bool MobileDeveloperJokerUsed { get; set; } = false;

    // Navigation properties
    public Player? Player { get; set; }

    /// <summary>
    /// List of answered questions in this session
    /// </summary>
    public ICollection<AnsweredQuestion> AnsweredQuestions { get; set; } = [];
}

