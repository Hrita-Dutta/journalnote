using System;
using System.Collections. Generic;

namespace JournalNote.Models
{
    public class EntryFilter
    {
        public string SearchTerm { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int> SelectedMoodIds { get; set; } = new();
        public List<int> SelectedTagIds { get; set; } = new();
        public string MoodCategory { get; set; } = string.Empty; // Positive, Neutral, Negative, All
        public bool HasActiveFilters => 
            ! string.IsNullOrWhiteSpace(SearchTerm) ||
            StartDate.HasValue ||
            EndDate.HasValue ||
            SelectedMoodIds. Count > 0 ||
            SelectedTagIds.Count > 0 ||
            ! string.IsNullOrEmpty(MoodCategory) && MoodCategory != "All";

        public void Clear()
        {
            SearchTerm = string.Empty;
            StartDate = null;
            EndDate = null;
            SelectedMoodIds.Clear();
            SelectedTagIds.Clear();
            MoodCategory = string.Empty;
        }
    }
}