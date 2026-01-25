using System;
using System.Threading.Tasks;
using JournalNote.Models;

namespace JournalNote.Services
{
    public class ThemeService
    {
        private readonly DatabaseService _databaseService;
        private AppTheme _currentTheme;

        public event EventHandler<AppTheme> ThemeChanged;

        public ThemeService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<AppTheme> GetCurrentThemeAsync()
        {
            if (_currentTheme != null)
                return _currentTheme;

            var settings = await _databaseService.GetThemeSettingsAsync();
            _currentTheme = await _databaseService.GetThemeByIdAsync(settings.SelectedThemeId);

            if (_currentTheme == null)
            {
                // Fallback to Light theme
                var themes = await _databaseService.GetAllThemesAsync();
                _currentTheme = themes[0];
            }

            return _currentTheme;
        }

        public async Task SetThemeAsync(int themeId)
        {
            var theme = await _databaseService.GetThemeByIdAsync(themeId);
            if (theme == null)
                return;

            _currentTheme = theme;

            var settings = await _databaseService.GetThemeSettingsAsync();
            settings.SelectedThemeId = themeId;
            await _databaseService.SaveThemeSettingsAsync(settings);

            ApplyTheme(theme);
            ThemeChanged?.Invoke(this, theme);
        }

        private void ApplyTheme(AppTheme theme)
        {
            // Apply theme colors to app resources
            Application.Current.Resources["PrimaryColor"] = Color.FromArgb(theme.PrimaryColor);
            Application.Current.Resources["SecondaryColor"] = Color.FromArgb(theme.SecondaryColor);
            Application.Current.Resources["BackgroundColor"] = Color.FromArgb(theme.BackgroundColor);
            Application.Current.Resources["SurfaceColor"] = Color.FromArgb(theme.SurfaceColor);
            Application.Current.Resources["TextColor"] = Color.FromArgb(theme.TextColor);
            Application.Current.Resources["TextSecondaryColor"] = Color.FromArgb(theme.TextSecondaryColor);
        }

        public async Task InitializeThemeAsync()
        {
            var theme = await GetCurrentThemeAsync();
            ApplyTheme(theme);
        }
    }
}