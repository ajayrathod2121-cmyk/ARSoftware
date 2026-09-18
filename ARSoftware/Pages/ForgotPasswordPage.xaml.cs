using Microsoft.Maui.Controls;

namespace ARSoftware.Pages
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        private async void OnSendOtpClicked(object sender, EventArgs e) => await DisplayAlert("OTP", "OnSendOtpClicked", "OK");
        private async void OnResendOtpClicked(object sender, EventArgs e) => await DisplayAlert("OTP", "OnResendOtpClicked", "OK");
        private async void OnVerifyOtpClicked(object sender, EventArgs e) => await DisplayAlert("Info", "OnVerifyOtpClicked", "OK");
        private async void OnBackToStep1Clicked(object sender, EventArgs e) => await DisplayAlert("Info", "OnBackToStep1Clicked", "OK");
        private async void OnBackToStep2Clicked(object sender, EventArgs e) => await DisplayAlert("Info", "OnBackToStep2Clicked", "OK");
        private async void OnResetClicked(object sender, EventArgs e) { await DisplayAlert("Info", "OnResetClicked", "OK"); await Shell.Current.GoToAsync("//LoginPage"); }
        private async void OnBackToDashboard(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Main");
        private async void OnBackToLoginClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//LoginPage");
        private async void OnNextClicked(object sender, EventArgs e) => await DisplayAlert("Info", "OnNextClicked", "OK");
        
        // ✅ આ જ Missing હતું Line 126 પર!
        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Success", "Password Changed!", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}