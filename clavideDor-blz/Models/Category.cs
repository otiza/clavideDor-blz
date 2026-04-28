namespace clavideDor_blz.Models;

public class Category
{
    public int Id { get; set; }

    /// <summary>
    /// Category ID from CSV (e.g., 1, 2, 3, 4, 5)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Category name (e.g., "Algorithmes", "Culture générale")
    /// </summary>
    public required string Name { get; set; }

    // Navigation properties
    /// <summary>
    /// Questions in this category
    /// </summary>
    public ICollection<Question> Questions { get; set; } = [];
}

