using SQLite;
using JournalNote.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            await _database. CreateTableAsync<JournalEntry>();
        }

        // ====== CREATE ======
        public async Task<int> AddJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();

            // Check if entry already exists for this date
            var existingEntry = await GetJournalEntryByDateAsync(journalEntry.Date);
            if (existingEntry != null)
            {
                throw new Exception("An entry already exists for this date.  Please update it instead.");
            }

            journalEntry.CreatedAt = DateTime.Now;
            journalEntry.UpdatedAt = DateTime.Now;

            return await _database.InsertAsync(journalEntry);
        }

        // ====== READ - Get journal entry by date ======
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

        // ====== READ - Get all journal entries ======
        public async Task<List<JournalEntry>> GetAllJournalEntriesAsync()
        {
            await InitAsync();
            
            return await _database.Table<JournalEntry>()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        // ====== READ - Get journal entry by ID ======
        public async Task<JournalEntry> GetJournalEntryByIdAsync(int id)
        {
            await InitAsync();
            
            return await _database.Table<JournalEntry>()
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
        }

        // ====== UPDATE ======
        public async Task<int> UpdateJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();
            
            journalEntry.UpdatedAt = DateTime.Now;
            
            return await _database.UpdateAsync(journalEntry);
        }

        // ====== DELETE ======
        public async Task<int> DeleteJournalEntryAsync(JournalEntry journalEntry)
        {
            await InitAsync();
            
            return await _database.DeleteAsync(journalEntry);
        }

        // ====== DELETE by ID ======
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

        // ====== Check if journal entry exists for date ======
        public async Task<bool> HasJournalEntryForDateAsync(string date)
        {
            await InitAsync();
            
            var journalEntry = await GetJournalEntryByDateAsync(date);
            return journalEntry != null;
        }

        // ====== Get total journal entry count ======
        public async Task<int> GetJournalEntryCountAsync()
        {
            await InitAsync();
            
            return await _database.Table<JournalEntry>().CountAsync();
        }
    }
}