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

        public bool IsPredefined { get; set; } // true for pre-built tags, false for custom

        public string Color { get; set; } = "#667eea"; // Tag color for UI

        public DateTime CreatedAt { get; set; }

        public Tag()
        {
            CreatedAt = DateTime.Now;
        }

        public Tag(string name, bool isPredefined, string color = "#667eea")
        {
            Name = name;
            IsPredefined = isPredefined;
            Color = color;
            CreatedAt = DateTime.Now;
        }
    }
}