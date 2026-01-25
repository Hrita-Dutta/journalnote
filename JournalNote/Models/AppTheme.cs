using System;

namespace JournalNote.Models
{
    public class AppTheme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string BackgroundColor { get; set; }
        public string SurfaceColor { get; set; }
        public string TextColor { get; set; }
        public string TextSecondaryColor { get; set; }
        public bool IsDark { get; set; }
        public bool IsPredefined { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ThemeSettings
    {
        public int Id { get; set; }
        public int SelectedThemeId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}