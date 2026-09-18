using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private GoogleDriveService _driveService = new GoogleDriveService();

        // 👑 SECRET - AR TRADING Static Logo માટે - Timer Logic
        int tapCount = 0;
        DateTime lastTapTime = DateTime.MinValue;

        public SettingsPage() { InitializeComponent(); }
        protected override void OnAppearing() { base.OnAppearing(); LoadSettings(); }

        // 🔒 આ છે SECRET LOGIC - AR TRADING Logo (applogo.png) પર - FilePicker નથી!
        private async void OnLogoTapped(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if ((now - lastTapTime).TotalSeconds > 2)
            {
                tapCount = 0; // 2 સેકન્ડ પછી Reset
            }
            lastTapTime = now;
            tapCount++;

            System.Diagnostics.Debug.WriteLine($"Secret Tap: {tapCount}/5");

            if (tapCount >= 5)
            {
                tapCount = 0;
                lastTapTime = DateTime.MinValue;
                try
                {
                    await Shell.Current.GoToAsync(nameof(AdminKeyGeneratorPage));
                }
                catch (Exception ex)
                {
                    try { await Shell.Current.Navigation.PushAsync(new AdminKeyGeneratorPage()); }
                    catch { await DisplayAlert("Error", $"Routing Error: {ex.Message}\nAppShell.xaml.cs માં Routing.RegisterRoute કરો!", "OK"); }
                }
                return;
            }
            // ✅ 1-4 Tap પર કઈ જ નહીં થાય - ફોલ્ડર નહીં ખુલે!
        }

        private void LoadSettings()
        {
            var b = BusinessService.GetBusiness();
            BusinessNameEntry.Text = string.IsNullOrWhiteSpace(b.BusinessName) ? Preferences.Get("BusinessName", "AR Software") : b.BusinessName;
            AddressEntry.Text = string.IsNullOrWhiteSpace(b.Address) ? Preferences.Get("BusinessAddress", "Bhavnagar, Gujarat") : b.Address;
            CityEntry.Text = string.IsNullOrWhiteSpace(b.City) ? Preferences.Get("BusinessCity", "Bhavnagar") : b.City;
            PincodeEntry.Text = string.IsNullOrWhiteSpace(b.Pincode) ? Preferences.Get("BusinessPincode", "364001") : b.Pincode;
            MobileEntry.Text = string.IsNullOrWhiteSpace(b.Mobile) ? Preferences.Get("BusinessMobile", "6005951768") : b.Mobile;
            EmailEntry.Text = string.IsNullOrWhiteSpace(b.Email) ? Preferences.Get("BusinessEmail", "ar@gmail.com") : b.Email;
            GstEntry.Text = string.IsNullOrWhiteSpace(b.GST) ? Preferences.Get("BusinessGst", "") : b.GST;
            BankNameEntry.Text = string.IsNullOrWhiteSpace(b.BankName) ? Preferences.Get("BankName", "") : b.BankName;
            AccountNoEntry.Text = string.IsNullOrWhiteSpace(b.AccountNo) ? Preferences.Get("AccountNo", "") : b.AccountNo;
            IfscEntry.Text = string.IsNullOrWhiteSpace(b.IFSC) ? Preferences.Get("IfscCode", "") : b.IFSC;
            AccountHolderEntry.Text = string.IsNullOrWhiteSpace(b.AccountHolder) ? Preferences.Get("AccountHolder", "") : b.AccountHolder;
            UpiIdEntry.Text = string.IsNullOrWhiteSpace(b.UPI) ? Preferences.Get("UpiId", "") : b.UPI;
            TermsEntry.Text = string.IsNullOrWhiteSpace(b.Terms) ? Preferences.Get("TermsConditions", "1. Payment Due in 7 Days") : b.Terms;

            BusinessNameLabel.Text = BusinessNameEntry.Text;
            BusinessAddressLabel.Text = AddressEntry.Text;

            AutoBackupSwitch.IsToggled = Preferences.Get("AutoBackup", true);
            DriveAutoBackupSwitch.IsToggled = Preferences.Get("GoogleDriveAutoBackup", false);

            bool isConnected = Preferences.Get("GoogleDriveConnected", false);
            DriveStatusLabel.Text = isConnected ? "✅ Connected" : "❌ Not Connected";
            DriveEmailLabel.Text = Preferences.Get("GoogleDriveEmail", "");
            DriveEmailLabel.IsVisible = isConnected;
            LastDriveBackupLabel.Text = $"Last Backup: {Preferences.Get("LastDriveBackup", "Never")}";

            string logoPath = Preferences.Get("BusinessLogoPath", Preferences.Get("biz_logo", ""));
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                LogoImage.Source = ImageSource.FromFile(logoPath);
                LogoImage.IsVisible = true; LogoText.IsVisible = false;
            }
            string qrPath = Preferences.Get("QrPath", Preferences.Get("biz_qr", ""));
            if (!string.IsNullOrEmpty(qrPath) && File.Exists(qrPath))
            {
                QrImage.Source = ImageSource.FromFile(qrPath);
                QrImage.IsVisible = true; QrText.IsVisible = false;
            }
        }

        private async void OnSaveSettings(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BusinessNameEntry.Text))
            {
                await DisplayAlert("Required", "1. કંપની નું નામ તો નાખો!", "OK");
                return;
            }
            var profile = new BusinessProfile
            {
                BusinessName = BusinessNameEntry.Text.Trim(),
                Address = AddressEntry.Text?.Trim() ?? "",
                City = CityEntry.Text?.Trim() ?? "Bhavnagar",
                Pincode = PincodeEntry.Text?.Trim() ?? "",
                Mobile = MobileEntry.Text?.Trim() ?? "",
                Email = EmailEntry.Text?.Trim() ?? "",
                GST = GstEntry.Text?.Trim() ?? "",
                BankName = BankNameEntry.Text?.Trim() ?? "",
                AccountNo = AccountNoEntry.Text?.Trim() ?? "",
                IFSC = IfscEntry.Text?.Trim() ?? "",
                AccountHolder = AccountHolderEntry.Text?.Trim() ?? "",
                UPI = UpiIdEntry.Text?.Trim() ?? "",
                Terms = TermsEntry.Text?.Trim() ?? "",
                LogoPath = Preferences.Get("BusinessLogoPath", ""),
                QrPath = Preferences.Get("QrPath", "")
            };
            BusinessService.SaveBusiness(profile);
            Preferences.Set("BusinessName", profile.BusinessName);
            Preferences.Set("BusinessAddress", profile.Address);
            Preferences.Set("BusinessCity", profile.City);
            Preferences.Set("BusinessPincode", profile.Pincode);
            Preferences.Set("BusinessMobile", profile.Mobile);
            Preferences.Set("BusinessEmail", profile.Email);
            Preferences.Set("BusinessGst", profile.GST);
            Preferences.Set("BankName", profile.BankName);
            Preferences.Set("AccountNo", profile.AccountNo);
            Preferences.Set("IfscCode", profile.IFSC);
            Preferences.Set("AccountHolder", profile.AccountHolder);
            Preferences.Set("UpiId", profile.UPI);
            Preferences.Set("TermsConditions", profile.Terms);

            SavedLabel.Text = $"✅ Saved! {profile.BusinessName}";
            SavedLabel.IsVisible = true;
            await DisplayAlert("Saved! ✅", $"{profile.BusinessName} Save થઈ ગયું!", "OK");
            await Task.Delay(800);
            SavedLabel.IsVisible = false;
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        // ✅ આ જ Business Logo Upload કરશે - જમણી બાજુ વાળો
        private async void OnLogoUpload(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select Logo" });
                if (result != null)
                {
                    string newPath = Path.Combine(FileSystem.AppDataDirectory, "logo" + Path.GetExtension(result.FileName));
                    using var stream = await result.OpenReadAsync();
                    using var newStream = File.OpenWrite(newPath);
                    await stream.CopyToAsync(newStream);
                    Preferences.Set("BusinessLogoPath", newPath);
                    Preferences.Set("biz_logo", newPath);
                    LogoImage.Source = ImageSource.FromFile(newPath);
                    LogoImage.IsVisible = true; LogoText.IsVisible = false;
                }
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnQrUpload(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select QR Code" });
                if (result != null)
                {
                    string newPath = Path.Combine(FileSystem.AppDataDirectory, "qr" + Path.GetExtension(result.FileName));
                    using var stream = await result.OpenReadAsync();
                    using var newStream = File.OpenWrite(newPath);
                    await stream.CopyToAsync(newStream);
                    Preferences.Set("QrPath", newPath);
                    QrImage.Source = ImageSource.FromFile(newPath);
                    QrImage.IsVisible = true; QrText.IsVisible = false;
                }
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
        private async void OnForgotPasswordClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
        private async void OnDeletedHistoryClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync(nameof(DeletedHistoryPage));
        private void OnAutoBackupToggled(object sender, ToggledEventArgs e) => Preferences.Set("AutoBackup", e.Value);
        private void OnDriveAutoBackupToggled(object sender, ToggledEventArgs e) => Preferences.Set("GoogleDriveAutoBackup", e.Value);

        private async void OnBackupNow(object sender, EventArgs e)
        {
            try
            {
                string path = await new BackupService().BackupNowAsync();
                await DisplayAlert("Backup", path, "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnConnectDrive(object sender, EventArgs e)
        {
            try
            {
                bool c = await _driveService.ConnectGoogleDriveAsync();
                if (c) Preferences.Set("GoogleDriveConnected", true);
                LoadSettings();
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnDriveBackupNow(object sender, EventArgs e)
        {
            try
            {
                await _driveService.AutoBackupToDriveAsync();
                Preferences.Set("LastDriveBackup", DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                LoadSettings();
                await DisplayAlert("Done", "Drive Backup Done", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnMobileSync(object sender, EventArgs e)
        {
            try
            {
                await _driveService.SyncFromDriveAsync();
                await DisplayAlert("Sync", "Sync Done", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
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

        private async void OnBackToDashboard(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Main");
        }
    }
}

