using ARSoftware.Services;
using ARSoftware.Pages;
using System.IO;

namespace ARSoftware
{
    public partial class App : Application
    {
        public static DatabaseService Database { get; } = new DatabaseService();

        private PeriodicTimer? _backupTimer;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public App()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    var ex = e.ExceptionObject as Exception;
                    LogToFile($"[AppDomain Unhandled] {ex}");
                };

                TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                    LogToFile($"[Task Unobserved] {e.Exception}");
                    e.SetObserved();
                };

                InitializeComponent();
                StartAutoBackupTimer();
            }
            catch (Exception ex)
            {
                LogToFile($"[App Constructor Crash] {ex}");
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                // ✅ LICENSE CHECK - ફક્ત આટલું જ એડ કર્યું
                var licenseService = new LicenseService();
                bool isValid = licenseService.IsLicenseValid();

                if (!isValid)
                {
                    // License નથી અથવા Expire થઈ ગયું - LicensePage ખોલો
                    LogToFile("[License] Invalid or Expired - Opening LicensePage");
                    return new Window(new NavigationPage(new LicensePage()));
                }
                else
                {
                    // License OK - Normal AppShell ખોલો
                    LogToFile("[License] Valid - Opening AppShell");
                    return new Window(new AppShell());
                }
            }
            catch (Exception ex)
            {
                LogToFile($"[CreateWindow Crash] {ex}");
                return new Window(new ContentPage { Content = new Label { Text = $"Crash: {ex.Message}" } });
            }
        }

        private void StartAutoBackupTimer()
        {
            try
            {
                _backupTimer = new PeriodicTimer(TimeSpan.FromHours(1));
                Task.Run(async () =>
                {
                    try
                    {
                        while (await _backupTimer.WaitForNextTickAsync(_cts.Token))
                        {
                            try
                            {
                                bool autoOn = Preferences.Get("GoogleDriveAutoBackup", false);
                                bool connected = Preferences.Get("GoogleDriveConnected", false);
                                if (autoOn && connected)
                                {
                                    var driveService = new GoogleDriveService();
                                    await driveService.AutoBackupToDriveAsync();
                                    Preferences.Set("LastDriveBackup", DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Auto Backup Error: {ex.Message}");
                                LogToFile($"[AutoBackup Error] {ex.Message}");
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        LogToFile($"[Timer Loop Crash] {ex}");
                    }
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                LogToFile($"[StartTimer Crash] {ex}");
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            try
            {
                if (Preferences.Get("GoogleDriveAutoBackup", false) && Preferences.Get("GoogleDriveConnected", false))
                {
                    Task.Run(async () => await new GoogleDriveService().AutoBackupToDriveAsync());
                }
            }
            catch (Exception ex)
            {
                LogToFile($"[OnSleep Error] {ex.Message}");
            }
        }

        // ✅ હવે STATIC કર્યું - એટલે બધે મળશે!
        public static void LogToFile(string message)
        {
            try
            {
                string path = @"D:\crash.txt";
                string log = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] {message}{Environment.NewLine}{Environment.NewLine}";
                File.AppendAllText(path, log);
                System.Diagnostics.Debug.WriteLine(log);
            }
            catch { }
        }
    }
}
