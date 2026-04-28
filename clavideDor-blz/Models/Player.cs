namespace clavideDor_blz.Models;

public class Player
{
    public int Id { get; set; }

    /// <summary>
    /// Player's name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Player's selected role (FrontDeveloper, BackDeveloper, MobileDeveloper)
    /// </summary>
    public required PlayerRole Role { get; set; }

    /// <summary>
    /// Date when the player was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// List of game sessions for this player
    /// </summary>
    public ICollection<GameSession> GameSessions { get; set; } = [];
}

