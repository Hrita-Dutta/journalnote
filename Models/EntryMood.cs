using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("entry_moods")]
    public class EntryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; }

        public int MoodId { get; set; }

        public bool IsPrimary { get; set; } // true for primary mood, false for secondary

        public EntryMood()
        {
        }
    }
}