using System;
using System.Collections.Generic;

namespace JournalNote.Models
{
    public class AnalyticsData
    {
        public MoodDistribution MoodStats { get; set; }
        public TagDistribution TagStats { get; set; }
        public WordCountTrends WordTrends { get; set; }
        public GeneralStats GeneralStats { get; set; }
    }

    public class MoodDistribution
    {
        public List<MoodCount> MoodCounts { get; set; }
        public string MostFrequentMood { get; set; }
        public int TotalMoodEntries { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; }
    }

    public class MoodCount
    {
        public string MoodName { get; set; }
        public string Category { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TagDistribution
    {
        public List<TagCount> TagCounts { get; set; }
        public string MostUsedTag { get; set; }
        public int TotalTagUsage { get; set; }
    }

    public class TagCount
    {
        public string TagName { get; set; }
        public string Color { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class WordCountTrends
    {
        public int AverageWordCount { get; set; }
        public int TotalWords { get; set; }
        public int ShortestEntry { get; set; }
        public int LongestEntry { get; set; }
        public List<WordCountByDate> DailyWordCounts { get; set; }
    }

    public class WordCountByDate
    {
        public DateTime Date { get; set; }
        public int WordCount { get; set; }
    }

    public class GeneralStats
    {
        public int TotalEntries { get; set; }
        public int DaysActive { get; set; }
        public double AverageEntriesPerWeek { get; set; }
        public DateTime FirstEntryDate { get; set; }
        public DateTime LastEntryDate { get; set; }
    }
}