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

        public string Content { get; set; } = string.Empty;

        public int?  PrimaryMoodId { get; set; }

        public string SecondaryMoodIds { get; set; } = string.Empty;

        public string TagIds { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

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