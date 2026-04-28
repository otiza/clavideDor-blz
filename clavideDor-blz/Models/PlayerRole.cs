namespace clavideDor_blz.Models;

/// <summary>
/// Player role enumeration with special abilities/jokers
/// </summary>
public enum PlayerRole
{
    /// <summary>
    /// Front Developer: Can change question once without penalty
    /// </summary>
    FrontDeveloper = 1,

    /// <summary>
    /// Back Developer: Gets one automatic second chance after wrong answer
    /// </summary>
    BackDeveloper = 2,

    /// <summary>
    /// Mobile Developer: Can reveal one hint
    /// </summary>
    MobileDeveloper = 3
}

