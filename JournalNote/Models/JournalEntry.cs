using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("journal_entries")]
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Indexed]
        [Column("entry_date")]
        public DateTime EntryDate { get; set; }

        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties for future features
        [Column("primary_mood")]
        public string?  PrimaryMood { get; set; }

        [Column("category")]
        public string? Category { get; set; }

        [Ignore]
        public bool IsToday => EntryDate.Date == DateTime.Today;
    }
}