using Microsoft.EntityFrameworkCore;

namespace clavideDor_blz.Data;

/// <summary>
/// Service to manage database initialization and operations
/// </summary>
public class DatabaseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseService> _logger;
    private readonly CsvQuestionSeeder _seeder;

    public DatabaseService(AppDbContext context, ILogger<DatabaseService> logger, CsvQuestionSeeder seeder)
    {
        _context = context;
        _logger = logger;
        _seeder = seeder;
    }

    /// <summary>
    /// Initialize the database and seed data if needed
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database...");

            // Create database if it doesn't exist
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");

            // Seed questions from CSV if database is empty
            await _seeder.SeedAsync();
            _logger.LogInformation("Database initialization complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization.");
            throw;
        }
    }

    /// <summary>
    /// Check if database is available
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}

