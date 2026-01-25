using SQLite;

namespace JournalNote.Models
{
    public class ThemeSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool IsDarkMode { get; set; }
    }
}