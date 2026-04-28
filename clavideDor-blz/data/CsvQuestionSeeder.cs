using clavideDor_blz.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace clavideDor_blz.Data;

/// <summary>
/// Loads questions from data/questions.csv and seeds them into the database
/// </summary>
public class CsvQuestionSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<CsvQuestionSeeder> _logger;

    // CSV record class for mapping
    private class QuestionCsvRecord
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string ChoiceA { get; set; } = string.Empty;
        public string ChoiceB { get; set; } = string.Empty;
        public string ChoiceC { get; set; } = string.Empty;
        public string ChoiceD { get; set; } = string.Empty;
        public string Correct { get; set; } = string.Empty;
        public bool IsBoss { get; set; }
    }

    // CSV class map for CsvHelper to correctly map column names
    private sealed class QuestionCsvRecordMap : ClassMap<QuestionCsvRecord>
    {
        public QuestionCsvRecordMap()
        {
            Map(m => m.CategoryId).Name("category_id");
            Map(m => m.CategoryName).Name("category_name");
            Map(m => m.Text).Name("text");
            Map(m => m.ChoiceA).Name("choice_a");
            Map(m => m.ChoiceB).Name("choice_b");
            Map(m => m.ChoiceC).Name("choice_c");
            Map(m => m.ChoiceD).Name("choice_d");
            Map(m => m.Correct).Name("correct");
            Map(m => m.IsBoss).Name("is_boss");
        }
    }

    public CsvQuestionSeeder(AppDbContext context, ILogger<CsvQuestionSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds questions from CSV file if database is empty
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            // Check if questions already exist
            if ((await _context.Questions.CountAsync()) > 0)
            {
                _logger.LogInformation("Database already seeded with questions.");
                return;
            }

            _logger.LogInformation("Starting to seed questions from CSV...");

            // Get CSV file path
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "questions.csv");

            if (!File.Exists(csvPath))
            {
                throw new FileNotFoundException($"CSV file not found at {csvPath}");
            }

            using var reader = new StreamReader(csvPath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<QuestionCsvRecordMap>();
            var records = csv.GetRecords<QuestionCsvRecord>().ToList();

            _logger.LogInformation($"Found {records.Count} records in CSV.");

            // Dictionary to track added categories
            var categories = new Dictionary<int, Category>();

            foreach (var record in records)
            {
                // Ensure category exists
                if (!categories.ContainsKey(record.CategoryId))
                {
                    var existingCategory = await _context.Categories
                        .Where(c => c.CategoryId == record.CategoryId)
                        .FirstOrDefaultAsync();

                    if (existingCategory == null)
                    {
                        var newCategory = new Category
                        {
                            CategoryId = record.CategoryId,
                            Name = record.CategoryName
                        };
                        _context.Categories.Add(newCategory);
                        await _context.SaveChangesAsync();
                        categories[record.CategoryId] = newCategory;
                        _logger.LogInformation($"Added category: {record.CategoryName}");
                    }
                    else
                    {
                        categories[record.CategoryId] = existingCategory;
                    }
                }

                // Create question
                var question = new Question
                {
                    CategoryId = record.CategoryId,
                    Text = record.Text,
                    ChoiceA = record.ChoiceA,
                    ChoiceB = record.ChoiceB,
                    ChoiceC = record.ChoiceC,
                    ChoiceD = record.ChoiceD,
                    Correct = record.Correct,
                    IsBoss = record.IsBoss
                };

                _context.Questions.Add(question);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully seeded {records.Count} questions from CSV.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding questions from CSV.");
            throw;
        }
    }
}

