using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("journal_entries")]
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Date { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // Store rich text as HTML
        public string Content { get; set; } = string.Empty;

        // Primary Mood (Required)
        public int?  PrimaryMoodId { get; set; }

        // Secondary Moods stored as comma-separated IDs (e.g., "2,5")
        public string SecondaryMoodIds { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public JournalEntry()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Date = DateTime.Today.ToString("yyyy-MM-dd");
        }
    }
}