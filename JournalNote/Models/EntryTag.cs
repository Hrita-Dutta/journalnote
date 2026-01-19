using SQLite;
using System;

namespace JournalNote.Models
{
    [Table("entry_tags")]
    public class EntryTag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; }

        public int TagId { get; set; }

        public DateTime CreatedAt { get; set; }

        public EntryTag()
        {
            CreatedAt = DateTime.Now;
        }
    }
}