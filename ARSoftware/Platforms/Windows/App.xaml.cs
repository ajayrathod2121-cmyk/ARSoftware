using Microsoft.UI.Xaml;
using System.IO;

namespace ARSoftware.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"D:\crash.txt", $"[WinUI App Constructor] {ex}{Environment.NewLine}");
                throw;
            }
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                base.OnLaunched(args);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"D:\crash.txt", $"[WinUI OnLaunched] {ex}{Environment.NewLine}");
                throw;
            }
        }
    }
}
