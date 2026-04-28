# Clavier d'Or

## Project goal
Clavier d'Or is a student-friendly quiz game built with **C#/.NET Blazor**. The app focuses on clean architecture with MVVM principles, persistent game sessions, role-based gameplay helpers (jokers), score history, and PDF export of results.

## Technologies used
- **.NET 10 / ASP.NET Core Blazor (Interactive Server)**
- **Entity Framework Core** (ORM)
- **SQLite** (local database)
- **CommunityToolkit.Mvvm** (MVVM helpers)
- **CsvHelper** (CSV parsing for seed data)
- **QuestPDF** (score report export)

## How to run
### Prerequisites
- .NET SDK installed (10.x recommended)

### Run steps
1. Restore packages:
   ```bash
   dotnet restore clavideDor-blz.sln
   ```
2. Build:
   ```bash
   dotnet build clavideDor-blz.sln
   ```
3. Start the app:
   ```bash
   dotnet run --project clavideDor-blz/clavideDor-blz.csproj
   ```
4. Open the local URL shown in the terminal (for example `https://localhost:xxxx`).

## Features
- Main menu with:
  - New Game
  - Resume Game (from unfinished sessions)
  - History
  - Quit
- Start a game with player name + role
- Quiz flow with categories and boss questions
- Save progress and resume later
- Finished game history (player, role, score, date)
- Result page with final statistics
- PDF export for score report

## Roles and jokers
### Front Developer
- Can change (skip) the current question **once** without penalty.

### Back Developer
- Gets one automatic second chance after a wrong answer (one-time retry behavior).

### Mobile Developer
- Can reveal one hint once.
- If no explicit hint exists, the app gives a useful fallback by hiding two wrong choices.

## Database / ORM
- SQLite database file is used for local persistence.
- EF Core models include:
  - `Player`
  - `GameSession`
  - `AnsweredQuestion`
  - `Question`
  - `Category`
  - `PlayerRole`
- On startup, the app initializes the database and runs seed logic.
- Seed data is loaded from the existing CSV file:
  - `clavideDor-blz/data/questions.csv`
- Seeder behavior is designed to avoid duplicate question insertion.

## PDF export
- PDF generation is handled by `PdfExportService` using QuestPDF.
- Export includes:
  - Title: **Clavier d'Or - Score**
  - Player name
  - Player role
  - Final score
  - Date
  - Number of answered questions
  - Categories completed
- From the Result page, exporting triggers a browser download of the generated PDF.
