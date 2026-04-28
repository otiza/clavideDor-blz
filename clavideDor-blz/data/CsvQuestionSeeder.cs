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
            _logger.LogInformation("Starting to seed questions from CSV...");

            // Get CSV file path
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "questions.csv");

            if (!File.Exists(csvPath))
            {
                throw new FileNotFoundException($"Could not seed data because questions.csv was not found at '{csvPath}'.");
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

            // Already existing categories/questions
            var categories = await _context.Categories
                .ToDictionaryAsync(c => c.CategoryId, c => c);

            var existingQuestionKeys = (await _context.Questions
                .Select(q => BuildQuestionKey(q.CategoryId, q.Text, q.ChoiceA, q.ChoiceB, q.ChoiceC, q.ChoiceD, q.Correct, q.IsBoss))
                .ToListAsync())
                .ToHashSet();

            var categoriesCreated = 0;
            var questionsAdded = 0;
            var duplicatesSkipped = 0;

            foreach (var record in records)
            {
                // Ensure category exists
                if (!categories.ContainsKey(record.CategoryId))
                {
                    var newCategory = new Category
                    {
                        CategoryId = record.CategoryId,
                        Name = record.CategoryName.Trim()
                    };
                    _context.Categories.Add(newCategory);
                    categories[record.CategoryId] = newCategory;
                    categoriesCreated++;
                }

                var questionKey = BuildQuestionKey(
                    record.CategoryId,
                    record.Text,
                    record.ChoiceA,
                    record.ChoiceB,
                    record.ChoiceC,
                    record.ChoiceD,
                    record.Correct,
                    record.IsBoss);

                if (existingQuestionKeys.Contains(questionKey))
                {
                    duplicatesSkipped++;
                    continue;
                }

                // Create question
                var question = new Question
                {
                    CategoryId = record.CategoryId,
                    Text = record.Text.Trim(),
                    ChoiceA = record.ChoiceA.Trim(),
                    ChoiceB = record.ChoiceB.Trim(),
                    ChoiceC = record.ChoiceC.Trim(),
                    ChoiceD = record.ChoiceD.Trim(),
                    Correct = record.Correct.Trim().ToUpperInvariant(),
                    IsBoss = record.IsBoss
                };

                _context.Questions.Add(question);
                existingQuestionKeys.Add(questionKey);
                questionsAdded++;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Seeding completed. Categories added: {CategoriesCreated}, Questions added: {QuestionsAdded}, Duplicates skipped: {DuplicatesSkipped}.",
                categoriesCreated,
                questionsAdded,
                duplicatesSkipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding questions from CSV.");
            throw;
        }
    }

    private static string BuildQuestionKey(
        int categoryId,
        string text,
        string choiceA,
        string choiceB,
        string choiceC,
        string choiceD,
        string correct,
        bool isBoss)
    {
        return string.Join(
            "|",
            categoryId,
            text.Trim(),
            choiceA.Trim(),
            choiceB.Trim(),
            choiceC.Trim(),
            choiceD.Trim(),
            correct.Trim().ToUpperInvariant(),
            isBoss);
    }
}
