using clavideDor_blz.Components;
using clavideDor_blz.Data;
using clavideDor_blz.Services;
using clavideDor_blz.ViewModels;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=claviedor.db"));

// Register data services
builder.Services.AddScoped<CsvQuestionSeeder>();
builder.Services.AddScoped<DatabaseService>();

// Register business logic services
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<PdfExportService>();

// Register ViewModels
builder.Services.AddScoped<MainMenuViewModel>();
builder.Services.AddScoped<NewGameViewModel>();
builder.Services.AddScoped<GameViewModel>();
builder.Services.AddScoped<HistoryViewModel>();
builder.Services.AddScoped<ResultViewModel>();

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
    try
    {
        await dbService.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
