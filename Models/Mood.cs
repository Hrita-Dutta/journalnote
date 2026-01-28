using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("moods")]
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public Mood()
        {
        }
    }
}