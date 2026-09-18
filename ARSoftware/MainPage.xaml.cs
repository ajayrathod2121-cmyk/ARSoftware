using ARSoftware.Pages;
using ARSoftware.Services;

namespace ARSoftware
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            if (WelcomeLabel != null)
                WelcomeLabel.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
        }

        private async void OnAavakClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(JobworkEntryPage)}");
        }

        private async void OnJavakClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(JavakEntryPage)}");
        }

        private async void OnPartyClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(PartyMasterPage)}");
        }

        private async void OnMoreClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("More Options", "Cancel", null, "Backup", "WhatsApp", "Update Check", "Settings", "Logout");

            if (action == "Backup") OnBackupClicked(sender, e);
            else if (action == "WhatsApp") OnWhatsAppClicked(sender, e);
            else if (action == "Update Check") OnUpdateClicked(sender, e);
            else if (action == "Logout") OnLogoutClicked(sender, e);
            else if (action == "Settings")
            {
                try { await Shell.Current.GoToAsync($"///{nameof(SettingsPage)}"); } catch { }
            }
        }

        // ✅ LOGOUT BUTTON FUNCTION - NAVU
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Logout?", "Bahar nikalvu che? Tamaro data safe che!", "Ha, Logout", "Cancel");
            if (!confirm) return;

            try
            {
                Preferences.Clear();
                try { SecureStorage.RemoveAll(); } catch { }

                // Supabase logout hoy to
                // await App.SupabaseService?.LogoutAsync();

                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnBackupClicked(object sender, EventArgs e)
        {
            try
            {
                bool result = await DisplayAlert("Backup", "Backup levu che?", "Ha", "Na");
                if (!result) return;
                string path = await new BackupService().BackupNowAsync();
                await DisplayAlert("Success", $"Backup thai gayu!\n{path}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnWhatsAppClicked(object sender, EventArgs e)
        {
            await WhatsAppService.SendBillAsync("9876543210", "Patel Bhai", "BILL-101", "5000");
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            await new UpdateService().CheckUpdateAsync(true);
        }
    
        // ✅ Back to Dashboard
        private async void OnBackClicked(object sender, EventArgs e)
        {
            try { await Shell.Current.GoToAsync("//DashboardPage"); }
            catch { try { await Navigation.PopAsync(); } catch { } }
        }
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = true });
        }
}
}

