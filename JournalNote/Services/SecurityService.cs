using System;
using System.Linq;
using System.Threading.Tasks;
using JournalNote.Models;

namespace JournalNote.Services
{
    public class SecurityService
    {
        private readonly DatabaseService _databaseService;
        private bool _isAuthenticated = false;

        public bool IsAuthenticated => _isAuthenticated;

        public SecurityService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> IsRegisteredAsync()
        {
            try
            {
                var settings = await _databaseService.GetSecuritySettingsAsync();
                return settings.IsRegistered;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsRegistered error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string pin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
                    return false;

                if (!pin.All(char.IsDigit))
                    return false;

                var settings = await _databaseService.GetSecuritySettingsAsync();
                
                settings.IsRegistered = true;
                settings.PinHash = pin;
                settings.CreatedAt = DateTime.Now;
                
                await _databaseService.SaveSecuritySettingsAsync(settings);
                _isAuthenticated = true;
                
                System.Diagnostics.Debug.WriteLine($"User registered with PIN: {pin}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginAsync(string pin)
        {
            try
            {
                var settings = await _databaseService.GetSecuritySettingsAsync();
                
                if (!settings.IsRegistered)
                {
                    System.Diagnostics.Debug.WriteLine("User not registered");
                    return false;
                }

                var isValid = pin == settings.PinHash;
                
                if (isValid)
                {
                    _isAuthenticated = true;
                    System.Diagnostics.Debug.WriteLine("Login successful");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Incorrect PIN");
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            _isAuthenticated = false;
            System.Diagnostics.Debug.WriteLine("User logged out");
        }
    }
}