using ARSoftware.Pages;
using ARSoftware.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace ARSoftware
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit() // ✅ FIX - અહીં જ હોવું જોઈએ - UseMauiApp પછી તરત
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services - તમારા જૂના
            builder.Services.AddSingleton<SupabaseService>();
            builder.Services.AddSingleton<PrintService>();

            // ✅ જૂના 3 Service - જેમના તેમ
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddSingleton<WhatsAppService>();
            builder.Services.AddSingleton<UpdateService>();

            // ✅ નવા 3 Service - Google Drive Direct + Mobile Sync + Delete History
            builder.Services.AddSingleton<GoogleDriveService>(); // દર 1 કલાકે Direct Drive Backup + 30 દિવસ
            builder.Services.AddSingleton<DeletedHistoryService>(); // Delete History - Recycle Bin
            

            // Pages - Main
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

            // Pages - Jobwork
            builder.Services.AddTransient<JobworkEntryPage>();
            builder.Services.AddTransient<JavakEntryPage>();
            builder.Services.AddTransient<BillsPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<AdminKeyGeneratorPage>();
            builder.Services.AddTransient<SettingsPage>();

            // ✅ નવા Pages
            builder.Services.AddTransient<DeletedHistoryPage>(); // Recycle Bin Page

            // Premium Pages
            try
            {
                builder.Services.AddTransient<PartyMasterPage>();
                builder.Services.AddTransient<PaymentPage>();
                builder.Services.AddTransient<ExpensePage>();
            }
            catch { }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}