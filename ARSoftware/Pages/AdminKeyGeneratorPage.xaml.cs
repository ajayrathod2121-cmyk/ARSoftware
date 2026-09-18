using ARSoftware.Services;
using System.Text;

namespace ARSoftware.Pages
{
    public partial class AdminKeyGeneratorPage : ContentPage
    {
        private LicenseService _licenseService = new();
        private List<object> _history = new();

        public AdminKeyGeneratorPage()
        {
            InitializeComponent();
            TypePicker.SelectedIndex = 0;
            LoadHistory();
        }

        private async void OnGenerateClicked(object sender, EventArgs e)
        {
            if (TypePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Type સિલેક્ટ કરો!", "OK");
                return;
            }

            string party = PartyNameEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(party)) party = "Client";

            // ✅ NEW: Mobile Number લો
            string mobile = MobileEntry.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(mobile) || mobile.Length != 10 || !mobile.All(char.IsDigit))
            {
                await DisplayAlert("Error", "📱 સાચો 10 Digit Mobile Number લખો!\nOTP એના પર જશે!", "OK");
                return;
            }

            string typeCode = TypePicker.SelectedIndex switch
            {
                0 => "DEMO",
                1 => "1M",
                2 => "6M",
                3 => "1Y",
                4 => "2Y",
                5 => "3Y",
                _ => "DEMO"
            };

            string key = _licenseService.GenerateKey(typeCode);
            var info = _licenseService.ValidateKey(key);

            GeneratedKeyLabel.Text = key;
            ExpiryInfoLabel.Text = $"{party} | {info.Type} | Expiry: {info.ExpiryDate:dd-MM-yyyy} | {info.DaysLeft} Days | 📱 {mobile}";
            ResultBorder.IsVisible = true;

            // ✅ Mobile સાથે Mapping Save કરો - OTP માટે
            Preferences.Set($"mobile_{key}", mobile);
            Preferences.Set($"party_{key}", party);

            // Save History
            _history.Insert(0, new { Party = party, Mobile = mobile, Key = key, Info = ExpiryInfoLabel.Text, Date = DateTime.Now });
            HistoryCollection.ItemsSource = null;
            HistoryCollection.ItemsSource = _history.Take(20).ToList();

            // Save to file D:\AR_Keys.txt also with Mobile
            try
            {
                string line = $"{DateTime.Now:dd-MM-yyyy HH:mm} | {party} | {mobile} | {typeCode} | {key} | {info.ExpiryDate:dd-MM-yyyy}\n";
                File.AppendAllText(@"D:\AR_Keys.txt", line);
            }
            catch { }

            await DisplayAlert("✅ Done", $"Key બની ગઈ!\n{key}\n\nParty: {party}\nMobile: {mobile}\n\nહવે WhatsApp બટન દબાવો, OTP {mobile} પર જશે!", "OK");
        }

        private async void OnCopyClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GeneratedKeyLabel.Text)) return;
            await Clipboard.SetTextAsync(GeneratedKeyLabel.Text);
            await DisplayAlert("Copied", "Key Copy થઈ ગઈ!", "OK");
        }

        private async void OnShareClicked(object sender, EventArgs e)
        {
            string mobile = MobileEntry.Text?.Trim() ?? "";
            string key = GeneratedKeyLabel.Text ?? "";
            string party = PartyNameEntry.Text?.Trim() ?? "Client";

            if (string.IsNullOrWhiteSpace(key))
            {
                await DisplayAlert("Error", "પહેલા Key Generate કરો!", "OK");
                return;
            }

            string msg = $"🔐 *AR SOFTWARE License Key*%0A%0AParty: {party}%0AMobile: {mobile}%0AKey: {key}%0A{ExpiryInfoLabel.Text}%0A%0AApp માં Key નાખો, OTP તમારા WhatsApp {mobile} પર આવશે!%0A%0A_AR TRADING Bhavnagar_";

            // ✅ સીધું Client ના WhatsApp પર મોકલો
            if (!string.IsNullOrWhiteSpace(mobile) && mobile.Length == 10)
            {
                await Launcher.OpenAsync($"https://wa.me/91{mobile}?text={msg}");
            }
            else
            {
                await Share.RequestAsync(new ShareTextRequest { Text = $"🔐 AR SOFTWARE License Key\n\nParty: {party}\nMobile: {mobile}\nKey: {key}\n{ExpiryInfoLabel.Text}\n\nAR TRADING Bhavnagar", Title = "License Key" });
            }
        }

        private void LoadHistory()
        {
            HistoryCollection.ItemsSource = _history;
        }

        private async void OnBackToDashboard(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Main");
        }
    }
}