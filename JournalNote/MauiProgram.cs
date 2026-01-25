using Microsoft.Extensions.Logging;
using JournalNote.Services;

namespace JournalNote;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        
        // Register DatabaseService as Singleton
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ThemeService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
       
#endif
        
        return builder.Build();
    }
}