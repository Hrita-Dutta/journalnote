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
            
            // Create all tables
            await _database.CreateTableAsync<JournalEntry>();
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<Tag>();

            // Seed initial data
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
                    return;

                var moods = new List<Mood>
                {
                    // Positive
                    new Mood { Name = "Happy", Category = "Positive", Icon = "😊" },
                    new Mood { Name = "Excited", Category = "Positive", Icon = "🤩" },
                    new Mood { Name = "Relaxed", Category = "Positive", Icon = "😌" },
                    new Mood { Name = "Grateful", Category = "Positive", Icon = "🙏" },
                    new Mood { Name = "Confident", Category = "Positive", Icon = "😎" },

                    // Neutral
                    new Mood { Name = "Calm", Category = "Neutral", Icon = "😐" },
                    new Mood { Name = "Thoughtful", Category = "Neutral", Icon = "🤔" },
                    new Mood { Name = "Curious", Category = "Neutral", Icon = "🧐" },
                    new Mood { Name = "Nostalgic", Category = "Neutral", Icon = "💭" },
                    new Mood { Name = "Bored", Category = "Neutral", Icon = "😑" },

                    // Negative
                    new Mood { Name = "Sad", Category = "Negative", Icon = "😢" },
                    new Mood { Name = "Angry", Category = "Negative", Icon = "😠" },
                    new Mood { Name = "Stressed", Category = "Negative", Icon = "😰" },
                    new Mood { Name = "Lonely", Category = "Negative", Icon = "😔" },
                    new Mood { Name = "Anxious", Category = "Negative", Icon = "😟" }
                };

                await _database.InsertAllAsync(moods);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding moods:  {ex.Message}");
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

        public async Task<List<Tag>> GetCustomTagsAsync()
        {
            await InitAsync();
            return await _database.Table<Tag>()
                .Where(t => t.IsPredefined == false)
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
                throw new Exception("A tag with this name already exists.");

            tag.CreatedAt = DateTime.Now;
            return await _database.InsertAsync(tag);
        }

        public async Task<int> DeleteTagAsync(Tag tag)
        {
            await InitAsync();
            
            if (tag. IsPredefined)
                throw new Exception("Cannot delete predefined tags.");

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
    }
}