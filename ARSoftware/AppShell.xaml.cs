using ARSoftware.Pages;
using Microsoft.Maui.Controls;

namespace ARSoftware;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ✅ તમારા જૂના Routes (જરૂરી છે)
        Routing.RegisterRoute(nameof(AdminKeyGeneratorPage), typeof(AdminKeyGeneratorPage));
        Routing.RegisterRoute(nameof(ItemMasterPage), typeof(ItemMasterPage));
        Routing.RegisterRoute(nameof(PartyReportPage), typeof(PartyReportPage));
        Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage));
        Routing.RegisterRoute(nameof(JobworkEntryPage), typeof(JobworkEntryPage));
        Routing.RegisterRoute(nameof(JavakEntryPage), typeof(JavakEntryPage));
        Routing.RegisterRoute(nameof(PartyMasterPage), typeof(PartyMasterPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(DeletedHistoryPage), typeof(DeletedHistoryPage));
        Routing.RegisterRoute(nameof(ExpensePage), typeof(ExpensePage));
        Routing.RegisterRoute(nameof(BillPage), typeof(BillPage));
        Routing.RegisterRoute("PartyMasterPage", typeof(Pages.PartyMasterPage));

        // ✅ NEW: Navigation Tracking for Back Button
        Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
    {
        // જો Dashboard નથી તો Back Logic અહીં ચલાવી શકાય
    }

    // ✅ Flyout Header માં Dashboard Button માટે - તમારા XAML માં Button છે તેના માટે
    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//Main");
            Shell.Current.FlyoutIsPresented = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ✅ દરેક Page પરથી Dashboard પર જવા માટે - Global
    public static async Task GoToDashboard()
    {
        await Shell.Current.GoToAsync("//Main");
    }
}