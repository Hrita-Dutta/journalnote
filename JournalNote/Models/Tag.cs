using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("tags")]
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Name { get; set; } = string.Empty;

        public bool IsPredefined { get; set; }

        public string Color { get; set; } = "#667eea";

        public DateTime CreatedAt { get; set; }

        public Tag()
        {
            CreatedAt = DateTime.Now;
        }
    }
}