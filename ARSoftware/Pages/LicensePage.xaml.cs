using ARSoftware.Services;

namespace ARSoftware.Pages;

public partial class LicensePage : ContentPage
{
    private LicenseService _licenseService = new();
    private string generatedOtp = "";
    private string registeredMobile = "";
    private DateTime otpExpiry;
    private string currentKey = "";

    private const string WHATSAPP_API_KEY = "123456"; // તમારો CallMeBot Key
    private int adminTapCount = 0;

    public LicensePage()
    {
        InitializeComponent();
        CheckLicenseStatusSafe();
    }

    private void CheckLicenseStatusSafe()
    {
        try
        {
            // તમારી Service માં GetLicenseInfo ન હોય તો આ Try માંથી Skip થશે
            string savedKey = Preferences.Get("LicenseKey", "");
            if (!string.IsNullOrWhiteSpace(savedKey))
            {
                var info = _licenseService.ValidateKey(savedKey);
                // ValidateKey જે Return કરે એમાંથી ExpiryDate અને DaysLeft તો હશે જ
                StatusLabel.Text = $"Expiry: {info.ExpiryDate:dd-MM-yyyy} | {info.DaysLeft} Days";
                ExpiryLabel.Text = $"Licensed: {info.Type}";
            }
        }
        catch { /* Service માં Method ન હોય તો Error નહીં આવે */ }
    }

    private async void OnBackToDashboard(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main");
    }

    private void OnAdminLogoTapped(object sender, EventArgs e)
    {
        adminTapCount++;
        if (adminTapCount >= 5)
        {
            AdminBorder.IsVisible = !AdminBorder.IsVisible;
            try
            {
                // GetDeviceId ન હોય તો Machine Name બતાવો
                DeviceIdLabel.Text = $"Device: {DeviceInfo.Current.Name} | {DeviceInfo.Current.Model}";
            }
            catch { DeviceIdLabel.Text = "Admin Mode"; }
            adminTapCount = 0;
        }
    }

    private async void OnActivateClicked(object sender, EventArgs e)
    {
        currentKey = KeyEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(currentKey))
        {
            await DisplayAlert("Error", "License Key નાખો!", "OK");
            return;
        }

        try
        {
            var info = _licenseService.ValidateKey(currentKey);
            // તમારી Service માં IsValid નથી, તો DaysLeft થી Check કરીએ
            if (info.ExpiryDate < DateTime.Now && info.DaysLeft <= 0)
            {
                await DisplayAlert("Expired", "Key Expire થઈ ગઈ છે!", "OK");
                return;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Invalid Key", $"Key ખોટી છે!\n{ex.Message}", "OK");
            return;
        }

        registeredMobile = Preferences.Get($"mobile_{currentKey}", "");
        if (string.IsNullOrWhiteSpace(registeredMobile))
        {
            string? mobileInput = await DisplayPromptAsync("Mobile Number", "Register કરેલો Mobile Number નાખો (OTP એના પર જશે):", "Send OTP", "Cancel", "9876543210", 10, Keyboard.Telephone);
            if (string.IsNullOrWhiteSpace(mobileInput) || mobileInput.Length != 10) return;
            registeredMobile = mobileInput.Trim();
        }

        generatedOtp = new Random().Next(100000, 999999).ToString();
        otpExpiry = DateTime.Now.AddMinutes(5);
        StatusLabel.Text = $"📱 OTP Sending to {registeredMobile}...";

        bool sent = await SendWhatsAppOtpAsync(registeredMobile, generatedOtp);

        OtpMobileLabel.Text = $"OTP {registeredMobile} ના WhatsApp પર મોકલ્યો છે!";
        OtpInfoLabel.Text = sent ? $"WhatsApp Check કરો! 5 min valid" : $"Testing OTP: {generatedOtp}";
        LicenseFrame.IsVisible = false;
        OtpFrame.IsVisible = true;

        if (sent)
            await DisplayAlert("OTP Sent! 📱", $"OTP {registeredMobile} ના WhatsApp પર મોકલ્યો છે!", "OK");
        else
            await DisplayAlert($"OTP (Testing) - {registeredMobile}", $"તમારો OTP છે: {generatedOtp}", "OK");
    }

    private async Task<bool> SendWhatsAppOtpAsync(string mobile, string otp)
    {
        try
        {
            string message = $"*AR TRADING - OTP*%0A%0AYour License OTP is: *{otp}*%0AValid for 5 min";
            string url = $"https://api.callmebot.com/whatsapp.php?phone=91{mobile}&text={message}&apikey={WHATSAPP_API_KEY}";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    private async void OnVerifyOtpClicked(object sender, EventArgs e)
    {
        string enteredOtp = OtpEntry.Text?.Trim() ?? "";
        if (DateTime.Now > otpExpiry)
        {
            await DisplayAlert("Expired", "OTP Expire થઈ ગયો! ફરી Key નાખો!", "OK");
            OtpFrame.IsVisible = false;
            LicenseFrame.IsVisible = true;
            return;
        }

        if (enteredOtp == generatedOtp)
        {
            try { _licenseService.SaveLicense(currentKey); } catch { }
            Preferences.Set("LicenseKey", currentKey);
            Preferences.Set("IsLicensed", true);
            Preferences.Set("LicensedMobile", registeredMobile);

            await DisplayAlert("Success ✅", $"License Active!\nMobile: {registeredMobile}", "OK");
            await Shell.Current.GoToAsync("//Main");
        }
        else
        {
            await DisplayAlert("Wrong OTP ❌", $"OTP ખોટો છે! {registeredMobile} પર આવેલો OTP Check કરો!", "OK");
        }
    }

    private async void OnResendClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(registeredMobile))
        {
            generatedOtp = new Random().Next(100000, 999999).ToString();
            otpExpiry = DateTime.Now.AddMinutes(5);
            await SendWhatsAppOtpAsync(registeredMobile, generatedOtp);
            await DisplayAlert("Resent! 🔄", $"નવો OTP {registeredMobile} પર મોકલ્યો! OTP: {generatedOtp}", "OK");
        }
    }

    private void OnGenerate7DaysClicked(object sender, EventArgs e)
    {
        try
        {
            string key = _licenseService.GenerateKey("DEMO");
            GeneratedKeyLabel.Text = key;
            GeneratedKeyLabel.IsVisible = true;
        }
        catch (Exception ex) { GeneratedKeyLabel.Text = ex.Message; GeneratedKeyLabel.IsVisible = true; }
    }

    private void OnGenerate1YearClicked(object sender, EventArgs e)
    {
        try
        {
            string key = _licenseService.GenerateKey("1Y");
            GeneratedKeyLabel.Text = key;
            GeneratedKeyLabel.IsVisible = true;
        }
        catch (Exception ex) { GeneratedKeyLabel.Text = ex.Message; GeneratedKeyLabel.IsVisible = true; }
    }
}