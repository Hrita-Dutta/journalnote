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
                await _database.CreateTableAsync<SecuritySettings>();
                

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

// ADD THIS METHOD HERE
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
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
        // ====== ANALYTICS METHODS ======
public async Task<AnalyticsData> GetAnalyticsDataAsync()
{
    await InitAsync();

    var entries = await _database.Table<JournalEntry>().ToListAsync();
    var moods = await _database.Table<Mood>().ToListAsync();
    var tags = await _database.Table<Tag>().ToListAsync();

    if (entries.Count == 0)
    {
        return new AnalyticsData
        {
            MoodStats = new MoodDistribution
            {
                MoodCounts = new List<MoodCount>(),
                MostFrequentMood = "N/A",
                TotalMoodEntries = 0,
                CategoryCounts = new Dictionary<string, int>()
            },
            TagStats = new TagDistribution
            {
                TagCounts = new List<TagCount>(),
                MostUsedTag = "N/A",
                TotalTagUsage = 0
            },
            WordTrends = new WordCountTrends
            {
                AverageWordCount = 0,
                TotalWords = 0,
                ShortestEntry = 0,
                LongestEntry = 0,
                DailyWordCounts = new List<WordCountByDate>()
            },
            GeneralStats = new GeneralStats
            {
                TotalEntries = 0,
                DaysActive = 0,
                AverageEntriesPerWeek = 0,
                FirstEntryDate = DateTime.Now,
                LastEntryDate = DateTime.Now
            }
        };
    }

    return new AnalyticsData
    {
        MoodStats = await CalculateMoodDistributionAsync(entries, moods),
        TagStats = await CalculateTagDistributionAsync(entries, tags),
        WordTrends = CalculateWordCountTrendsAsync(entries),
        GeneralStats = CalculateGeneralStatsAsync(entries)
    };
}

private async Task<MoodDistribution> CalculateMoodDistributionAsync(List<JournalEntry> entries, List<Mood> moods)
{
    var moodCounts = new Dictionary<int, int>();
    var categoryCounts = new Dictionary<string, int>
    {
        { "Positive", 0 },
        { "Neutral", 0 },
        { "Negative", 0 }
    };

    // Count primary moods
    foreach (var entry in entries)
    {
        if (entry.PrimaryMoodId.HasValue)
        {
            if (!moodCounts.ContainsKey(entry.PrimaryMoodId.Value))
                moodCounts[entry.PrimaryMoodId.Value] = 0;
            
            moodCounts[entry.PrimaryMoodId.Value]++;
        }

        // Count secondary moods
        if (!string.IsNullOrEmpty(entry.SecondaryMoodIds))
        {
            var secondaryIds = entry.SecondaryMoodIds.Split(',');
            foreach (var idStr in secondaryIds)
            {
                if (int.TryParse(idStr.Trim(), out int moodId))
                {
                    if (!moodCounts.ContainsKey(moodId))
                        moodCounts[moodId] = 0;
                    
                    moodCounts[moodId]++;
                }
            }
        }
    }

    var totalMoodCount = moodCounts.Values.Sum();
    var moodCountList = new List<MoodCount>();

    foreach (var kvp in moodCounts.OrderByDescending(x => x.Value))
    {
        var mood = moods.FirstOrDefault(m => m.Id == kvp.Key);
        if (mood != null)
        {
            moodCountList.Add(new MoodCount
            {
                MoodName = mood.Name,
                Category = mood.Category,
                Count = kvp.Value,
                Percentage = totalMoodCount > 0 ? (kvp.Value * 100.0 / totalMoodCount) : 0
            });

            // Count by category
            if (categoryCounts.ContainsKey(mood.Category))
                categoryCounts[mood.Category] += kvp.Value;
        }
    }

    var mostFrequent = moodCountList.FirstOrDefault();

    return new MoodDistribution
    {
        MoodCounts = moodCountList,
        MostFrequentMood = mostFrequent?.MoodName ?? "N/A",
        TotalMoodEntries = totalMoodCount,
        CategoryCounts = categoryCounts
    };
}

private async Task<TagDistribution> CalculateTagDistributionAsync(List<JournalEntry> entries, List<Tag> tags)
{
    var tagCounts = new Dictionary<int, int>();

    foreach (var entry in entries)
    {
        if (!string.IsNullOrEmpty(entry.TagIds))
        {
            var tagIds = entry.TagIds.Split(',');
            foreach (var idStr in tagIds)
            {
                if (int.TryParse(idStr.Trim(), out int tagId))
                {
                    if (!tagCounts.ContainsKey(tagId))
                        tagCounts[tagId] = 0;
                    
                    tagCounts[tagId]++;
                }
            }
        }
    }

    var totalTagCount = tagCounts.Values.Sum();
    var tagCountList = new List<TagCount>();

    foreach (var kvp in tagCounts.OrderByDescending(x => x.Value))
    {
        var tag = tags.FirstOrDefault(t => t.Id == kvp.Key);
        if (tag != null)
        {
            tagCountList.Add(new TagCount
            {
                TagName = tag.Name,
                Color = tag.Color,
                Count = kvp.Value,
                Percentage = totalTagCount > 0 ? (kvp.Value * 100.0 / totalTagCount) : 0
            });
        }
    }

    var mostUsed = tagCountList.FirstOrDefault();

    return new TagDistribution
    {
        TagCounts = tagCountList,
        MostUsedTag = mostUsed?.TagName ?? "N/A",
        TotalTagUsage = totalTagCount
    };
}

private WordCountTrends CalculateWordCountTrendsAsync(List<JournalEntry> entries)
{
    var wordCounts = new List<int>();
    var dailyWordCounts = new List<WordCountByDate>();

    foreach (var entry in entries)
    {
        var content = System.Text.RegularExpressions.Regex.Replace(entry.Content ?? "", "<.*?>", string.Empty);
        var wordCount = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        
        wordCounts.Add(wordCount);
        dailyWordCounts.Add(new WordCountByDate
        {
            Date = DateTime.Parse(entry.Date),
            WordCount = wordCount
        });
    }

    var totalWords = wordCounts.Sum();
    var avgWords = wordCounts.Count > 0 ? (int)(totalWords / (double)wordCounts.Count) : 0;

    return new WordCountTrends
    {
        AverageWordCount = avgWords,
        TotalWords = totalWords,
        ShortestEntry = wordCounts.Count > 0 ? wordCounts.Min() : 0,
        LongestEntry = wordCounts.Count > 0 ? wordCounts.Max() : 0,
        DailyWordCounts = dailyWordCounts.OrderBy(x => x.Date).ToList()
    };
}

private GeneralStats CalculateGeneralStatsAsync(List<JournalEntry> entries)
{
    var dates = entries.Select(e => DateTime.Parse(e.Date)).OrderBy(d => d).ToList();
    var firstDate = dates.First();
    var lastDate = dates.Last();
    var totalDays = (lastDate - firstDate).Days + 1;
    var totalWeeks = totalDays / 7.0;
    var avgPerWeek = totalWeeks > 0 ? entries.Count / totalWeeks : entries.Count;

    return new GeneralStats
    {
        TotalEntries = entries.Count,
        DaysActive = dates.Distinct().Count(),
        AverageEntriesPerWeek = Math.Round(avgPerWeek, 1),
        FirstEntryDate = firstDate,
        LastEntryDate = lastDate
    };
}

// ====== SECURITY SETTINGS METHODS ======
public async Task<SecuritySettings> GetSecuritySettingsAsync()
{
    try
    {
        await InitAsync();
        var settings = await _database.Table<SecuritySettings>().FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new SecuritySettings
            {
                IsRegistered = false,
                PinHash = string.Empty,
                CreatedAt = DateTime.Now
            };
            await _database.InsertAsync(settings);
        }
        
        return settings;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error in GetSecuritySettingsAsync: {ex.Message}");
        return new SecuritySettings
        {
            IsRegistered = false,
            PinHash = string.Empty,
            CreatedAt = DateTime.Now
        };
    }
}

public async Task<int> SaveSecuritySettingsAsync(SecuritySettings settings)
{
    try
    {
        await InitAsync();
        
        var existing = await _database.Table<SecuritySettings>().FirstOrDefaultAsync();
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
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error in SaveSecuritySettingsAsync: {ex.Message}");
        return 0;
    }
}
        
    }
}
