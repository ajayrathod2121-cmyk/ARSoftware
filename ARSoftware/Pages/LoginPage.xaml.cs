namespace ARSoftware.Pages
{
    public partial class LoginPage : ContentPage
    {
        private bool _isPasswordVisible = false;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnShowPasswordClicked(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            var mobilePass = this.FindByName("PasswordEntryMobile") as Entry;
            var pcPass = this.FindByName("PasswordEntry") as Entry;
            if (mobilePass != null) mobilePass.IsPassword = !_isPasswordVisible;
            if (pcPass != null) pcPass.IsPassword = !_isPasswordVisible;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var mobileUser = this.FindByName("UsernameEntryMobile") as Entry;
            var pcUser = this.FindByName("UsernameEntry") as Entry;
            var mobilePass = this.FindByName("PasswordEntryMobile") as Entry;
            var pcPass = this.FindByName("PasswordEntry") as Entry;
            var mobileError = this.FindByName("ErrorLabelMobile") as Label;
            var pcError = this.FindByName("ErrorLabel") as Label;

            string username = (pcUser?.Text ?? mobileUser?.Text ?? "").Trim();
            string password = (pcPass?.Text ?? mobilePass?.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                if (mobileError != null) { mobileError.Text = "Username and Password required!"; mobileError.IsVisible = true; }
                if (pcError != null) { pcError.Text = "Username and Password required!"; pcError.IsVisible = true; }
                return;
            }

            if (mobileError != null) mobileError.IsVisible = false;
            if (pcError != null) pcError.IsVisible = false;

            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnForgotClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ForgotPasswordPage");
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

