using SQLite;
using JournalNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppTheme = JournalNote.Models.AppTheme;

namespace JournalNote.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        }

        private async Task InitAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_databasePath);
            
            // Create all tables
            await _database.CreateTableAsync<JournalEntry>();
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<AppTheme>();
            await _database.CreateTableAsync<ThemeSettings>();

            // Seed initial data
            await SeedMoodsAsync();
            await SeedTagsAsync();
            await SeedPredefinedThemesAsync(); 
        }

        // ====== SEED MOODS ======
        // ====== SEED MOODS ======
        private async Task SeedMoodsAsync()
        {
            try
            {
                var count = await _database.Table<Mood>().CountAsync();
                if (count > 0)
                    return;

                var moods = new List<Mood>
                {
                    // Positive Moods
                    new Mood { Name = "Happy", Category = "Positive", Icon = "" },
                    new Mood { Name = "Excited", Category = "Positive", Icon = "" },
                    new Mood { Name = "Relaxed", Category = "Positive", Icon = "" },
                    new Mood { Name = "Grateful", Category = "Positive", Icon = "" },
                    new Mood { Name = "Confident", Category = "Positive", Icon = "" },

                    // Neutral Moods
                    new Mood { Name = "Calm", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Thoughtful", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Curious", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Nostalgic", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Bored", Category = "Neutral", Icon = "" },

                    // Negative Moods
                    new Mood { Name = "Sad", Category = "Negative", Icon = "" },
                    new Mood { Name = "Angry", Category = "Negative", Icon = "" },
                    new Mood { Name = "Stressed", Category = "Negative", Icon = "" },
                    new Mood { Name = "Lonely", Category = "Negative", Icon = "" },
                    new Mood { Name = "Anxious", Category = "Negative", Icon = "" }
                };

                await _database.InsertAllAsync(moods);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding moods: {ex.Message}");
            }
        }

        // ====== SEED TAGS ======
        private async Task SeedTagsAsync()
        {
            try
            {
                var count = await _database.Table<Tag>().CountAsync();
                if (count > 0)
                    return;

                var tags = new List<Tag>
                {
                    // Work & Career
                    new Tag { Name = "Work", IsPredefined = true, Color = "#3498db" },
                    new Tag { Name = "Career", IsPredefined = true, Color = "#2980b9" },
                    new Tag { Name = "Studies", IsPredefined = true, Color = "#9b59b6" },
                    new Tag { Name = "Projects", IsPredefined = true, Color = "#8e44ad" },
                    new Tag { Name = "Planning", IsPredefined = true, Color = "#34495e" },

                    // Relationships
                    new Tag { Name = "Family", IsPredefined = true, Color = "#e74c3c" },
                    new Tag { Name = "Friends", IsPredefined = true, Color = "#c0392b" },
                    new Tag { Name = "Relationships", IsPredefined = true, Color = "#e67e22" },
                    new Tag { Name = "Parenting", IsPredefined = true, Color = "#d35400" },

                    // Health & Wellness
                    new Tag { Name = "Health", IsPredefined = true, Color = "#1abc9c" },
                    new Tag { Name = "Fitness", IsPredefined = true, Color = "#16a085" },
                    new Tag { Name = "Exercise", IsPredefined = true, Color = "#27ae60" },
                    new Tag { Name = "Meditation", IsPredefined = true, Color = "#2ecc71" },
                    new Tag { Name = "Yoga", IsPredefined = true, Color = "#27ae60" },
                    new Tag { Name = "Self-care", IsPredefined = true, Color = "#1abc9c" },

                    // Personal Growth
                    new Tag { Name = "Personal Growth", IsPredefined = true, Color = "#f39c12" },
                    new Tag { Name = "Reflection", IsPredefined = true, Color = "#f1c40f" },
                    new Tag { Name = "Spirituality", IsPredefined = true, Color = "#e67e22" },

                    // Hobbies
                    new Tag { Name = "Hobbies", IsPredefined = true, Color = "#9b59b6" },
                    new Tag { Name = "Reading", IsPredefined = true, Color = "#8e44ad" },
                    new Tag { Name = "Writing", IsPredefined = true, Color = "#3498db" },
                    new Tag { Name = "Music", IsPredefined = true, Color = "#e91e63" },
                    new Tag { Name = "Cooking", IsPredefined = true, Color = "#ff5722" },
                    new Tag { Name = "Shopping", IsPredefined = true, Color = "#ff9800" },

                    // Travel & Nature
                    new Tag { Name = "Travel", IsPredefined = true, Color = "#00bcd4" },
                    new Tag { Name = "Vacation", IsPredefined = true, Color = "#03a9f4" },
                    new Tag { Name = "Nature", IsPredefined = true, Color = "#4caf50" },

                    // Events
                    new Tag { Name = "Birthday", IsPredefined = true, Color = "#e91e63" },
                    new Tag { Name = "Holiday", IsPredefined = true, Color = "#f44336" },
                    new Tag { Name = "Celebration", IsPredefined = true, Color = "#ff5722" },

                    // Finance
                    new Tag { Name = "Finance", IsPredefined = true, Color = "#607d8b" }
                };

                await _database.InsertAllAsync(tags);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding tags: {ex.Message}");
            }
        }

        // ====== JOURNAL ENTRY CRUD ======
        public async Task<int> AddJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();

            var existingEntry = await GetJournalEntryByDateAsync(journalEntry.Date);
            if (existingEntry != null)
                throw new Exception("An entry already exists for this date.  Please update it instead.");

            journalEntry.CreatedAt = DateTime.Now;
            journalEntry.UpdatedAt = DateTime.Now;

            return await _database.InsertAsync(journalEntry);
        }

        public async Task<JournalEntry> GetJournalEntryByDateAsync(string date)
        {
            await InitAsync();
            
            try
            {
                return await _database.Table<JournalEntry>()
                    .Where(e => e.Date == date)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<JournalEntry>> GetAllJournalEntriesAsync()
        {
            await InitAsync();
            
            return await _database.Table<JournalEntry>()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<JournalEntry> GetJournalEntryByIdAsync(int id)
        {
            await InitAsync();
            
            return await _database.Table<JournalEntry>()
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();
            
            journalEntry.UpdatedAt = DateTime.Now;
            
            return await _database.UpdateAsync(journalEntry);
        }

        public async Task<int> DeleteJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();
            return await _database.DeleteAsync(journalEntry);
        }

        public async Task<int> DeleteJournalEntryByIdAsync(int id)
        {
            await InitAsync();
            
            var journalEntry = await GetJournalEntryByIdAsync(id);
            if (journalEntry != null)
                return await _database.DeleteAsync(journalEntry);
            
            return 0;
        }

        public async Task<bool> HasJournalEntryForDateAsync(string date)
        {
            await InitAsync();
            var journalEntry = await GetJournalEntryByDateAsync(date);
            return journalEntry != null;
        }

        public async Task<int> GetJournalEntryCountAsync()
        {
            await InitAsync();
            return await _database.Table<JournalEntry>().CountAsync();
        }

        // ====== MOOD METHODS ======
        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            await InitAsync();
            return await _database.Table<Mood>().ToListAsync();
        }

        public async Task<List<Mood>> GetMoodsByCategoryAsync(string category)
        {
            await InitAsync();
            return await _database.Table<Mood>()
                .Where(m => m.Category == category)
                .ToListAsync();
        }

        public async Task<Mood> GetMoodByIdAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Mood>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Mood>> GetSecondaryMoodsForEntryAsync(JournalEntry entry)
        {
            await InitAsync();

            if (string.IsNullOrEmpty(entry.SecondaryMoodIds))
                return new List<Mood>();

            var moodIds = entry.SecondaryMoodIds
                .Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.Parse(s.Trim()))
                .ToList();

            var moods = new List<Mood>();
            foreach (var id in moodIds)
            {
                var mood = await GetMoodByIdAsync(id);
                if (mood != null)
                    moods.Add(mood);
            }

            return moods;
        }

        // ====== TAG METHODS ======
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetPredefinedTagsAsync()
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .Where(t => t.IsPredefined == true)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
        

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .Where(t => t.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }

        


        public async Task<List<Tag>> GetTagsForEntryAsync(JournalEntry entry)
        {
            await InitAsync();

            if (string.IsNullOrEmpty(entry.TagIds))
                return new List<Tag>();

            var tagIds = entry.TagIds
                .Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => int.Parse(s.Trim()))
                .ToList();

            var tags = new List<Tag>();
            foreach (var id in tagIds)
            {
                var tag = await GetTagByIdAsync(id);
                if (tag != null)
                    tags.Add(tag);
            }

            return tags;
        }

        // DEBUG/MAINTENANCE METHODS 
        public async Task ForceReseedMoodsAsync()
        {
            await InitAsync();
            await _database.DeleteAllAsync<Mood>();
            await SeedMoodsAsync();
        }

        public async Task ForceReseedTagsAsync()
        {
            await InitAsync();
            await _database.DeleteAllAsync<Tag>();
            await SeedTagsAsync();
        }

        public async Task<string> GetDatabasePathAsync()
        {
            await InitAsync();
            return _databasePath;
        }
        
        // ====== STREAK TRACKING ======
public async Task<StreakInfo> GetStreakInfoAsync()
{
    await InitAsync();

    var allEntries = await GetAllJournalEntriesAsync();
    
    if (!allEntries.Any())
    {
        return new StreakInfo
        {
            CurrentStreak = 0,
            LongestStreak = 0,
            TotalEntries = 0,
            MissedDays = 0,
            LastEntryDate = null,
            FirstEntryDate = null,
            CompletionRate = 0
        };
    }

    var entryDates = allEntries
        .Select(e => DateTime.Parse(e.Date).Date)
        .OrderBy(d => d)
        .ToList();

    var totalEntries = entryDates.Count;
    var firstEntryDate = entryDates.First();
    var lastEntryDate = entryDates.Last();

    // Calculate current streak
    var currentStreak = 0;
    var today = DateTime.Today.Date;
    var checkDate = today;

    // Check if there's an entry today or yesterday to start counting
    if (entryDates.Contains(today) || entryDates.Contains(today.AddDays(-1)))
    {
        // Start from yesterday if no entry today
        if (!entryDates.Contains(today))
        {
            checkDate = today.AddDays(-1);
        }

        while (entryDates.Contains(checkDate))
        {
            currentStreak++;
            checkDate = checkDate.AddDays(-1);
        }
    }

    // Calculate longest streak
    var longestStreak = 0;
    var tempStreak = 1;

    for (int i = 1; i < entryDates.Count; i++)
    {
        var daysDifference = (entryDates[i] - entryDates[i - 1]).Days;

        if (daysDifference == 1)
        {
            tempStreak++;
        }
        else
        {
            if (tempStreak > longestStreak)
            {
                longestStreak = tempStreak;
            }
            tempStreak = 1;
        }
    }

    if (tempStreak > longestStreak)
    {
        longestStreak = tempStreak;
    }

    // Calculate missed days
    var totalDaysSinceStart = (today - firstEntryDate).Days + 1;
    var missedDays = totalDaysSinceStart - totalEntries;

    // Calculate completion rate
    var completionRate = totalDaysSinceStart > 0 
        ? Math.Round((totalEntries / (double)totalDaysSinceStart) * 100, 1) 
        : 0;

    return new StreakInfo
    {
        CurrentStreak = currentStreak,
        LongestStreak = longestStreak,
        TotalEntries = totalEntries,
        MissedDays = missedDays > 0 ? missedDays : 0,
        LastEntryDate = lastEntryDate,
        FirstEntryDate = firstEntryDate,
        CompletionRate = completionRate
    };
}

// ====== THEME MANAGEMENT METHODS ======
public async Task<List<AppTheme>> GetAllThemesAsync()
{
    await InitAsync();
    return await _database.Table<AppTheme>().ToListAsync();
}

public async Task<AppTheme> GetThemeByIdAsync(int id)
{
    await InitAsync();
    return await _database.Table<AppTheme>()
        .Where(t => t.Id == id)
        .FirstOrDefaultAsync();
}

public async Task<int> SaveThemeAsync(AppTheme theme)
{
    await InitAsync();
    
    if (theme.Id != 0)
    {
        return await _database.UpdateAsync(theme);
    }
    else
    {
        theme.CreatedAt = DateTime.Now;
        return await _database.InsertAsync(theme);
    }
}

public async Task<int> DeleteThemeAsync(int id)
{
    await InitAsync();
    return await _database.DeleteAsync<AppTheme>(id);
}

public async Task<ThemeSettings> GetThemeSettingsAsync()
{
    await InitAsync();
    var settings = await _database.Table<ThemeSettings>().FirstOrDefaultAsync();
    
    if (settings == null)
    {
        // Create default settings
        settings = new ThemeSettings
        {
            SelectedThemeId = 1, // Default to Light theme
            UpdatedAt = DateTime.Now
        };
        await _database.InsertAsync(settings);
    }
    
    return settings;
}

public async Task<int> SaveThemeSettingsAsync(ThemeSettings settings)
{
    await InitAsync();
    settings.UpdatedAt = DateTime.Now;
    
    var existing = await _database.Table<ThemeSettings>().FirstOrDefaultAsync();
    if (existing != null)
    {
        settings.Id = existing.Id;
        return await _database.UpdateAsync(settings);
    }
    else
    {
        return await _database.InsertAsync(settings);
    }
}

public async Task SeedPredefinedThemesAsync()
{
    await InitAsync();
    
    var existingThemes = await _database.Table<AppTheme>().CountAsync();
    if (existingThemes > 0)
        return; // Themes already seeded
    
    var themes = new List<AppTheme>
    {
        // Light Theme
        new AppTheme
        {
            Name = "Light",
            PrimaryColor = "#4A90E2",
            SecondaryColor = "#6C757D",
            BackgroundColor = "#F8F9FA",
            SurfaceColor = "#FFFFFF",
            TextColor = "#343A40",
            TextSecondaryColor = "#6C757D",
            IsDark = false,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        },
        
        // Dark Theme
        new AppTheme
        {
            Name = "Dark",
            PrimaryColor = "#5C7CFA",
            SecondaryColor = "#ADB5BD",
            BackgroundColor = "#1A1A1A",
            SurfaceColor = "#2D2D2D",
            TextColor = "#E9ECEF",
            TextSecondaryColor = "#ADB5BD",
            IsDark = true,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        },
        
        // Blue Theme
        new AppTheme
        {
            Name = "Ocean Blue",
            PrimaryColor = "#0077B6",
            SecondaryColor = "#00B4D8",
            BackgroundColor = "#E3F2FD",
            SurfaceColor = "#FFFFFF",
            TextColor = "#01497C",
            TextSecondaryColor = "#0096C7",
            IsDark = false,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        },
        
        // Purple Theme
        new AppTheme
        {
            Name = "Purple Dream",
            PrimaryColor = "#7950F2",
            SecondaryColor = "#9775FA",
            BackgroundColor = "#F3F0FF",
            SurfaceColor = "#FFFFFF",
            TextColor = "#5F3DC4",
            TextSecondaryColor = "#7950F2",
            IsDark = false,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        },
        
        // Green Theme
        new AppTheme
        {
            Name = "Forest Green",
            PrimaryColor = "#2F9E44",
            SecondaryColor = "#51CF66",
            BackgroundColor = "#EBFBEE",
            SurfaceColor = "#FFFFFF",
            TextColor = "#2B8A3E",
            TextSecondaryColor = "#37B24D",
            IsDark = false,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        },
        
        // Midnight Theme
        new AppTheme
        {
            Name = "Midnight",
            PrimaryColor = "#748FFC",
            SecondaryColor = "#91A7FF",
            BackgroundColor = "#0F0F1E",
            SurfaceColor = "#1A1B2E",
            TextColor = "#E5E5E5",
            TextSecondaryColor = "#B8B8D1",
            IsDark = true,
            IsPredefined = true,
            CreatedAt = DateTime.Now
        }
    };
    
    foreach (var theme in themes)
    {
        await _database.InsertAsync(theme);
    }
}


    }
}