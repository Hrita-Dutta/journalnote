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
            await _database. CreateTableAsync<EntryMood>();

            // Seed moods if table is empty
            await SeedMoodsAsync();
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
                    return; // Already seeded
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
                
                var newCount = await _database.Table<Mood>().CountAsync();
                Console.WriteLine($"Moods seeded successfully!  Total count: {newCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding moods:  {ex.Message}");
            }
        }

        // ====== MOOD METHODS ======
        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            await InitAsync();
            var moods = await _database.Table<Mood>().ToListAsync();
            Console.WriteLine($"GetAllMoodsAsync returned {moods.Count} moods");
            return moods;
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

        // ====== FORCE RESEED MOODS (for debugging) ======
        public async Task ForceReseedMoodsAsync()
        {
            await InitAsync();
            
            // Delete all existing moods
            await _database.DeleteAllAsync<Mood>();
            
            // Reseed
            await SeedMoodsAsync();
        }

        // ====== JOURNAL ENTRY METHODS ======
        public async Task<int> AddJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();

            var existingEntry = await GetJournalEntryByDateAsync(journalEntry.Date);
            if (existingEntry != null)
            {
                throw new Exception("An entry already exists for this date.  Please update it instead.");
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

        // ====== HELPER METHODS FOR MOODS ======
        public async Task<List<Mood>> GetSecondaryMoodsForEntryAsync(JournalEntry entry)
        {
            await InitAsync();

            if (string.IsNullOrEmpty(entry.SecondaryMoodIds))
                return new List<Mood>();

            var moodIds = entry.SecondaryMoodIds
                .Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(id => int.Parse(id. Trim()))
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