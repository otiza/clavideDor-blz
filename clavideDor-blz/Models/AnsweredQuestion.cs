namespace clavideDor_blz.Models;

public class AnsweredQuestion
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to GameSession
    /// </summary>
    public int GameSessionId { get; set; }

    /// <summary>
    /// Foreign key to Question
    /// </summary>
    public int QuestionId { get; set; }

    /// <summary>
    /// Player's selected answer (A, B, C, or D)
    /// </summary>
    public required string SelectedAnswer { get; set; }

    /// <summary>
    /// Is the answer correct?
    /// </summary>
    public bool IsCorrect { get; set; } = false;

    /// <summary>
    /// Points earned from this question
    /// Correct normal question = 10
    /// Correct boss question = 15
    /// Wrong = 0
    /// </summary>
    public int PointsEarned { get; set; } = 0;

    /// <summary>
    /// Date when the answer was submitted
    /// </summary>
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public GameSession? GameSession { get; set; }
    public Question? Question { get; set; }
}

