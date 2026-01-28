using System;
using System.Collections.Generic;

namespace JournalNote.Models
{
    public class ExportRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeMoods { get; set; }
        public bool IncludeTags { get; set; }
    }

    public class ExportEntry
    {
        public string Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PrimaryMood { get; set; }
        public List<string> SecondaryMoods { get; set; }
        public List<string> Tags { get; set; }
    }
}