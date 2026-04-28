namespace clavideDor_blz.Models;

public class Question
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Category
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Question text
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// Choice A
    /// </summary>
    public required string ChoiceA { get; set; }

    /// <summary>
    /// Choice B
    /// </summary>
    public required string ChoiceB { get; set; }

    /// <summary>
    /// Choice C
    /// </summary>
    public required string ChoiceC { get; set; }

    /// <summary>
    /// Choice D
    /// </summary>
    public required string ChoiceD { get; set; }

    /// <summary>
    /// Correct answer (A, B, C, or D)
    /// </summary>
    public required string Correct { get; set; }

    /// <summary>
    /// Is this a boss question?
    /// </summary>
    public bool IsBoss { get; set; } = false;

    // Navigation properties
    public Category? Category { get; set; }

    /// <summary>
    /// Answered questions (records of player answers)
    /// </summary>
    public ICollection<AnsweredQuestion> AnsweredQuestions { get; set; } = [];
}

