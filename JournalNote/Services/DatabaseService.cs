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
        private SQLiteAsyncConnection?  _database;
        private readonly string _dbPath;

        public DatabaseService()
        {
            // Set database path based on platform
            _dbPath = Path. Combine(
                Environment.GetFolderPath(Environment. SpecialFolder.LocalApplicationData),
                "journalnote.db3"
            );
        }

        // Initialize database connection
        private async Task InitAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);
            await _database.CreateTableAsync<JournalEntry>();
        }

        // Get entry for a specific date
        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            await InitAsync();
            var dateOnly = date.Date;
            return await _database.Table<JournalEntry>()
                .Where(e => e.EntryDate.Date == dateOnly)
                .FirstOrDefaultAsync();
        }

        // Get today's entry
        public async Task<JournalEntry?> GetTodayEntryAsync()
        {
            return await GetEntryByDateAsync(DateTime.Today);
        }

        // Create or Update entry
        public async Task<int> SaveEntryAsync(JournalEntry entry)
        {
            await InitAsync();

            // Ensure only one entry per day
            var existingEntry = await GetEntryByDateAsync(entry.EntryDate);

            if (entry.Id != 0)
            {
                // Update existing entry
                entry.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(entry);
            }
            else
            {
                if (existingEntry != null)
                {
                    // Update the existing entry instead of creating new
                    existingEntry.Title = entry.Title;
                    existingEntry. Content = entry.Content;
                    existingEntry.UpdatedAt = DateTime.Now;
                    return await _database.UpdateAsync(existingEntry);
                }

                // Create new entry
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                entry.EntryDate = entry.EntryDate.Date; // Normalize to date only
                return await _database.InsertAsync(entry);
            }
        }

        // Delete entry
        public async Task<int> DeleteEntryAsync(JournalEntry entry)
        {
            await InitAsync();
            return await _database.DeleteAsync(entry);
        }

        // Get all entries (for future list view)
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            await InitAsync();
            return await _database.Table<JournalEntry>()
                .OrderByDescending(e => e. EntryDate)
                .ToListAsync();
        }

        // Get entries count
        public async Task<int> GetEntriesCountAsync()
        {
            await InitAsync();
            return await _database.Table<JournalEntry>().CountAsync();
        }
    }
}