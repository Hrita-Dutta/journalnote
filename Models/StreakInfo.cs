using System;

namespace JournalNote.Models
{
    public class StreakInfo
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalEntries { get; set; }
        public int MissedDays { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public DateTime? FirstEntryDate { get; set; }
        public double CompletionRate { get; set; }
    }
}