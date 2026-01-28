using System;
using System.Threading.Tasks;
using JournalNote.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace JournalNote.Services
{
    public class ThemeService
    {
        private readonly DatabaseService _databaseService;
        private bool _isDarkMode;
        private bool _isInitialized = false;

        public event EventHandler ThemeChanged;

        public ThemeService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task InitializeThemeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                var isDarkMode = await GetIsDarkModeAsync();
                ApplyTheme(isDarkMode);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing theme: {ex.Message}");
                // Fallback to light theme
                ApplyTheme(false);
            }
        }

        public async Task<bool> GetIsDarkModeAsync()
        {
            try
            {
                var settings = await _databaseService.GetThemeSettingsAsync();
                _isDarkMode = settings?.IsDarkMode ?? false;
                return _isDarkMode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting dark mode: {ex.Message}");
                return false;
            }
        }

        public async Task SetDarkModeAsync(bool isDarkMode)
        {
            try
            {
                _isDarkMode = isDarkMode;

                var settings = await _databaseService.GetThemeSettingsAsync();
                if (settings != null)
                {
                    settings.IsDarkMode = isDarkMode;
                    await _databaseService.SaveThemeSettingsAsync(settings);
                }

                ApplyTheme(isDarkMode);
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting dark mode: {ex.Message}");
            }
        }

        private void ApplyTheme(bool isDarkMode)
        {
            try
            {
                if (Application.Current?.Resources == null)
                {
                    System.Diagnostics.Debug.WriteLine("Application.Current.Resources is null");
                    return;
                }

                if (isDarkMode)
                {
                    // Dark Theme
                    Application.Current.Resources["PrimaryColor"] = Color.FromArgb("#5C7CFA");
                    Application.Current.Resources["SecondaryColor"] = Color.FromArgb("#ADB5BD");
                    Application.Current.Resources["BackgroundColor"] = Color.FromArgb("#1A1A1A");
                    Application.Current.Resources["SurfaceColor"] = Color.FromArgb("#2D2D2D");
                    Application.Current.Resources["TextColor"] = Color.FromArgb("#E9ECEF");
                    Application.Current.Resources["TextSecondaryColor"] = Color.FromArgb("#ADB5BD");
                    Application.Current.Resources["BorderColor"] = Color.FromArgb("#495057");
                    Application.Current.Resources["SidebarColor"] = Color.FromArgb("#212529");
                }
                else
                {
                    // Light Theme
                    Application.Current.Resources["PrimaryColor"] = Color.FromArgb("#4A90E2");
                    Application.Current.Resources["SecondaryColor"] = Color.FromArgb("#6C757D");
                    Application.Current.Resources["BackgroundColor"] = Color.FromArgb("#F8F9FA");
                    Application.Current.Resources["SurfaceColor"] = Color.FromArgb("#FFFFFF");
                    Application.Current.Resources["TextColor"] = Color.FromArgb("#343A40");
                    Application.Current.Resources["TextSecondaryColor"] = Color.FromArgb("#6C757D");
                    Application.Current.Resources["BorderColor"] = Color.FromArgb("#DEE2E6");
                    Application.Current.Resources["SidebarColor"] = Color.FromArgb("#343A40");
                }

                System.Diagnostics.Debug.WriteLine($"Theme applied successfully: {(isDarkMode ? "Dark" : "Light")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            }
        }

        public async Task ToggleThemeAsync()
        {
            var currentMode = await GetIsDarkModeAsync();
            await SetDarkModeAsync(!currentMode);
        }
    }
}