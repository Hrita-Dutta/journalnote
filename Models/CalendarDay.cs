using System;

namespace JournalNote.Models
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasEntry { get; set; }
        public bool IsSelected { get; set; }
        public int DayOfMonth => Date.Day;
        public string FormattedDate => Date.ToString("yyyy-MM-dd");
    }
}