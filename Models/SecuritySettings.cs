using SQLite;
using System;

namespace JournalNote.Models
{
    public class SecuritySettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public bool IsRegistered { get; set; }
        
        public string PinHash { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}