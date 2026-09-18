namespace ARSoftware.Pages;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage()
    {
        InitializeComponent();
    }

    private void OnNewPasswordChanged(object sender, TextChangedEventArgs e)
    {
        var pwd = e.NewTextValue ?? "";
        if (pwd.Length == 0) { StrengthLabel.Text = "Strength: -"; StrengthLabel.TextColor = Color.FromArgb("#888888"); }
        else if (pwd.Length < 4) { StrengthLabel.Text = "Strength: Weak 🔴"; StrengthLabel.TextColor = Colors.Red; }
        else if (pwd.Length < 8) { StrengthLabel.Text = "Strength: Medium 🟡"; StrengthLabel.TextColor = Color.FromArgb("#D4AF37"); }
        else { StrengthLabel.Text = "Strength: Strong 🟢"; StrengthLabel.TextColor = Color.FromArgb("#22C55E"); }
    }

    private void OnToggleOldPassword(object sender, TappedEventArgs e)
    {
        OldPassEntry.IsPassword = !OldPassEntry.IsPassword;
    }
    private void OnToggleNewPassword(object sender, TappedEventArgs e)
    {
        NewPassEntry.IsPassword = !NewPassEntry.IsPassword;
        ConfirmPassEntry.IsPassword = NewPassEntry.IsPassword;
    }

    private async void OnUpdatePasswordClicked(object sender, TappedEventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(OldPassEntry.Text) || string.IsNullOrWhiteSpace(NewPassEntry.Text) || string.IsNullOrWhiteSpace(ConfirmPassEntry.Text))
        {
            ErrorLabel.Text = "❌ બધા Field ભરો!";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (NewPassEntry.Text != ConfirmPassEntry.Text)
        {
            ErrorLabel.Text = "❌ New Password Match નથી થતો!";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (NewPassEntry.Text.Length < 4)
        {
            ErrorLabel.Text = "❌ Password 4 અક્ષરથી મોટો હોવો જોઈએ!";
            ErrorLabel.IsVisible = true;
            return;
        }

        // ✅ અહીં SQLite Check કરો
        // var currentUser = Preferences.Get("CurrentUsername", "admin");
        // bool isOldCorrect = await App.Database.VerifyPasswordAsync(currentUser, OldPassEntry.Text);
        // if (!isOldCorrect) { ErrorLabel.Text = "❌ જૂનો Password ખોટો છે!"; ErrorLabel.IsVisible = true; return; }

        // await App.Database.UpdatePasswordAsync(currentUser, NewPassEntry.Text);

        // Demo Success
        await DisplayAlert("Success", $"✅ Password Update થઈ ગયો!\nNew: {NewPassEntry.Text}", "OK");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackClicked(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnBackToDashboard(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Main");
    }
}
