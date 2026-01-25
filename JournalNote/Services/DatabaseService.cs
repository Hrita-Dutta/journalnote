using SQLite;
using JournalNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace JournalNote.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _databasePath;

        // prevents double-init if multiple callers hit InitAsync at the same time
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _isInitialized;

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        }

        private async Task InitAsync()
        {
            if (_isInitialized && _database != null)
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized && _database != null)
                    return;

                _database = new SQLiteAsyncConnection(_databasePath);

                // Create all tables
                await _database.CreateTableAsync<JournalEntry>();
                await _database.CreateTableAsync<Mood>();
                await _database.CreateTableAsync<Tag>();
                await _database.CreateTableAsync<ThemeSettings>();

                // Seed initial data (these methods MUST NOT call InitAsync)
                await SeedMoodsAsync();
                await SeedTagsAsync();
                

                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        // ====== SEED MOODS ======
        // NOTE: does NOT call InitAsync (called by InitAsync already)
        private async Task SeedMoodsAsync()
        {
            try
            {
                var count = await _database.Table<Mood>().CountAsync();
                if (count > 0)
                    return;

                var moods = new List<Mood>
                {
                    // Positive
                    new Mood { Name = "Happy", Category = "Positive", Icon = "" },
                    new Mood { Name = "Excited", Category = "Positive", Icon = "" },
                    new Mood { Name = "Relaxed", Category = "Positive", Icon = "" },
                    new Mood { Name = "Grateful", Category = "Positive", Icon = "" },
                    new Mood { Name = "Confident", Category = "Positive", Icon = "" },

                    // Neutral
                    new Mood { Name = "Calm", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Thoughtful", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Curious", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Nostalgic", Category = "Neutral", Icon = "" },
                    new Mood { Name = "Bored", Category = "Neutral", Icon = "" },

                    // Negative
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
        // NOTE: does NOT call InitAsync (called by InitAsync already)
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
                throw new Exception("An entry already exists for this date. Please update it instead.");

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
            return await _database.Table<JournalEntry>().Where(e => e.Date == date).FirstOrDefaultAsync() != null;
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

            // SAFER parsing (skip bad dates)
            var entryDates = allEntries
                .Select(e =>
                {
                    if (DateTime.TryParse(e.Date, out var dt)) return (DateTime?)dt.Date;
                    return null;
                })
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .OrderBy(d => d)
                .ToList();

            if (!entryDates.Any())
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

            var totalEntries = entryDates.Count;
            var firstEntryDate = entryDates.First();
            var lastEntryDate = entryDates.Last();

            // Current streak
            var currentStreak = 0;
            var today = DateTime.Today.Date;
            var checkDate = today;

            if (entryDates.Contains(today) || entryDates.Contains(today.AddDays(-1)))
            {
                if (!entryDates.Contains(today))
                    checkDate = today.AddDays(-1);

                while (entryDates.Contains(checkDate))
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
            }

            // Longest streak
            var longestStreak = 1;
            var tempStreak = 1;

            for (int i = 1; i < entryDates.Count; i++)
            {
                var diff = (entryDates[i] - entryDates[i - 1]).Days;
                if (diff == 1)
                {
                    tempStreak++;
                }
                else
                {
                    if (tempStreak > longestStreak)
                        longestStreak = tempStreak;

                    tempStreak = 1;
                }
            }

            if (tempStreak > longestStreak)
                longestStreak = tempStreak;

            // Missed days / completion
            var totalDaysSinceStart = (today - firstEntryDate).Days + 1;
            var missedDays = totalDaysSinceStart - totalEntries;

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
        
        
// ====== THEME SETTINGS METHODS ======
        public async Task<ThemeSettings> GetThemeSettingsAsync()
        {
            try
            {
                await InitAsync();
                var settings = await _database.Table<ThemeSettings>().FirstOrDefaultAsync();
        
                if (settings == null)
                {
                    // Create default settings (Light mode)
                    settings = new ThemeSettings
                    {
                        IsDarkMode = false
                    };
                    await _database.InsertAsync(settings);
                    System.Diagnostics.Debug.WriteLine("Created default theme settings");
                }
        
                return settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetThemeSettingsAsync: {ex.Message}");
                // Return default settings on error
                return new ThemeSettings { IsDarkMode = false };
            }
        }

        public async Task<int> SaveThemeSettingsAsync(ThemeSettings settings)
        {
            try
            {
                await InitAsync();
        
                if (settings == null)
                {
                    System.Diagnostics.Debug.WriteLine("ThemeSettings is null");
                    return 0;
                }
        
                var existing = await _database.Table<ThemeSettings>().FirstOrDefaultAsync();
                if (existing != null)
                {
                    settings.Id = existing.Id;
                    var result = await _database.UpdateAsync(settings);
                    System.Diagnostics.Debug.WriteLine($"Updated theme settings: IsDarkMode={settings.IsDarkMode}");
                    return result;
                }
                else
                {
                    var result = await _database.InsertAsync(settings);
                    System.Diagnostics.Debug.WriteLine($"Inserted theme settings: IsDarkMode={settings.IsDarkMode}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SaveThemeSettingsAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return 0;
            }
        }

        
    }
}
