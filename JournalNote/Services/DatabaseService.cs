using SQLite;
using JournalNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            // Create tables
            await _database.CreateTableAsync<JournalEntry>();
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<EntryMood>();
            await _database.CreateTableAsync<EntryTag>();

            // Seed data
            await SeedMoodsAsync();
            await SeedTagsAsync();
        }

        // ====== SEED MOODS ======
        private async Task SeedMoodsAsync()
        {
            try
            {
                var count = await _database.Table<Mood>().CountAsync();

                if (count > 0)
                {
                    Console.WriteLine($"Moods already seeded. Count: {count}");
                    return;
                }

                var moods = new List<Mood>
                {
                    // Positive Moods
                    new Mood { Name = "Happy", Category = "Positive", Icon = "😊" },
                    new Mood { Name = "Excited", Category = "Positive", Icon = "🤩" },
                    new Mood { Name = "Relaxed", Category = "Positive", Icon = "😌" },
                    new Mood { Name = "Grateful", Category = "Positive", Icon = "🙏" },
                    new Mood { Name = "Confident", Category = "Positive", Icon = "😎" },

                    // Neutral Moods
                    new Mood { Name = "Calm", Category = "Neutral", Icon = "😐" },
                    new Mood { Name = "Thoughtful", Category = "Neutral", Icon = "🤔" },
                    new Mood { Name = "Curious", Category = "Neutral", Icon = "🧐" },
                    new Mood { Name = "Nostalgic", Category = "Neutral", Icon = "💭" },
                    new Mood { Name = "Bored", Category = "Neutral", Icon = "😑" },

                    // Negative Moods
                    new Mood { Name = "Sad", Category = "Negative", Icon = "😢" },
                    new Mood { Name = "Angry", Category = "Negative", Icon = "😠" },
                    new Mood { Name = "Stressed", Category = "Negative", Icon = "😰" },
                    new Mood { Name = "Lonely", Category = "Negative", Icon = "😔" },
                    new Mood { Name = "Anxious", Category = "Negative", Icon = "😟" }
                };

                await _database.InsertAllAsync(moods);
                Console.WriteLine($"Moods seeded successfully! Total: {moods.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding moods: {ex.Message}");
            }
        }

        // ====== SEED TAGS ======
        private async Task SeedTagsAsync()
        {
            try
            {
                var count = await _database.Table<Tag>().CountAsync();

                if (count > 0)
                {
                    Console.WriteLine($"Tags already seeded. Count: {count}");
                    return;
                }

                var predefinedTags = new List<Tag>
                {
                    // Work & Career
                    new Tag("Work", true, "#3498db"),
                    new Tag("Career", true, "#2980b9"),
                    new Tag("Studies", true, "#9b59b6"),
                    new Tag("Projects", true, "#8e44ad"),
                    new Tag("Planning", true, "#34495e"),

                    // Relationships
                    new Tag("Family", true, "#e74c3c"),
                    new Tag("Friends", true, "#c0392b"),
                    new Tag("Relationships", true, "#e67e22"),
                    new Tag("Parenting", true, "#d35400"),

                    // Health & Wellness
                    new Tag("Health", true, "#1abc9c"),
                    new Tag("Fitness", true, "#16a085"),
                    new Tag("Exercise", true, "#27ae60"),
                    new Tag("Meditation", true, "#2ecc71"),
                    new Tag("Yoga", true, "#27ae60"),
                    new Tag("Self-care", true, "#1abc9c"),

                    // Personal Growth
                    new Tag("Personal Growth", true, "#f39c12"),
                    new Tag("Reflection", true, "#f1c40f"),
                    new Tag("Spirituality", true, "#e67e22"),

                    // Hobbies & Activities
                    new Tag("Hobbies", true, "#9b59b6"),
                    new Tag("Reading", true, "#8e44ad"),
                    new Tag("Writing", true, "#3498db"),
                    new Tag("Music", true, "#e91e63"),
                    new Tag("Cooking", true, "#ff5722"),
                    new Tag("Shopping", true, "#ff9800"),

                    // Travel & Nature
                    new Tag("Travel", true, "#00bcd4"),
                    new Tag("Vacation", true, "#03a9f4"),
                    new Tag("Nature", true, "#4caf50"),

                    // Events
                    new Tag("Birthday", true, "#e91e63"),
                    new Tag("Holiday", true, "#f44336"),
                    new Tag("Celebration", true, "#ff5722"),

                    // Finance
                    new Tag("Finance", true, "#607d8b")
                };

                await _database.InsertAllAsync(predefinedTags);
                Console.WriteLine($"Tags seeded successfully! Total: {predefinedTags.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding tags: {ex.Message}");
            }
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
                .Where(t => t.IsPredefined)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetCustomTagsAsync()
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .Where(t => !t.IsPredefined)
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

        public async Task<int> AddTagAsync(Tag tag)
        {
            await InitAsync();

            var existing = await GetTagByNameAsync(tag.Name);
            if (existing != null)
            {
                throw new Exception("A tag with this name already exists.");
            }

            return await _database.InsertAsync(tag);
        }

        public async Task<int> DeleteTagAsync(Tag tag)
        {
            await InitAsync();

            if (tag.IsPredefined)
            {
                throw new Exception("Cannot delete predefined tags.");
            }

            return await _database.DeleteAsync(tag);
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

        // ====== JOURNAL ENTRY METHODS ======
        public async Task<int> AddJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();

            var existingEntry = await GetJournalEntryByDateAsync(journalEntry.Date);
            if (existingEntry != null)
            {
                throw new Exception("An entry already exists for this date. Please update it instead.");
            }

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
            {
                return await _database.DeleteAsync(journalEntry);
            }
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

        public async Task<List<Mood>> GetSecondaryMoodsForEntryAsync(JournalEntry entry)
        {
            await InitAsync();

            if (string.IsNullOrEmpty(entry.SecondaryMoodIds))
                return new List<Mood>();

            var moodIds = entry.SecondaryMoodIds
                .Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(id => int.Parse(id.Trim()))
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
    }
}
