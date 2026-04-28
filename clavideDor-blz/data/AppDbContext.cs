using clavideDor_blz.Models;
using Microsoft.EntityFrameworkCore;

namespace clavideDor_blz.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<AnsweredQuestion> AnsweredQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player configuration
        modelBuilder.Entity<Player>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Player>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);
        modelBuilder.Entity<Player>()
            .HasMany(p => p.GameSessions)
            .WithOne(g => g.Player)
            .HasForeignKey(g => g.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category configuration
        modelBuilder.Entity<Category>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.CategoryId)
            .IsUnique();
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Questions)
            .WithOne(q => q.Category)
            .HasForeignKey(q => q.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Question configuration
        modelBuilder.Entity<Question>()
            .HasKey(q => q.Id);
        modelBuilder.Entity<Question>()
            .Property(q => q.Text)
            .IsRequired();
        modelBuilder.Entity<Question>()
            .Property(q => q.ChoiceA)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Question>()
            .Property(q => q.ChoiceB)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Question>()
            .Property(q => q.ChoiceC)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Question>()
            .Property(q => q.ChoiceD)
            .IsRequired()
            .HasMaxLength(500);
        modelBuilder.Entity<Question>()
            .Property(q => q.Correct)
            .IsRequired()
            .HasMaxLength(1);
        modelBuilder.Entity<Question>()
            .HasMany(q => q.AnsweredQuestions)
            .WithOne(aq => aq.Question)
            .HasForeignKey(aq => aq.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // GameSession configuration
        modelBuilder.Entity<GameSession>()
            .HasKey(g => g.Id);
        modelBuilder.Entity<GameSession>()
            .HasMany(g => g.AnsweredQuestions)
            .WithOne(aq => aq.GameSession)
            .HasForeignKey(aq => aq.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // AnsweredQuestion configuration
        modelBuilder.Entity<AnsweredQuestion>()
            .HasKey(aq => aq.Id);
        modelBuilder.Entity<AnsweredQuestion>()
            .Property(aq => aq.SelectedAnswer)
            .IsRequired()
            .HasMaxLength(1);
    }
}

