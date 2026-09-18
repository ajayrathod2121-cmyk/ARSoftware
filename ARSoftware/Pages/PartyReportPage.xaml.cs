using Microsoft.Maui.Controls;

namespace ARSoftware.Pages
{
    // આ Line સૌથી અગત્યની છે - આનાથી જ Route માંથી PartyId મળશે!
    [QueryProperty(nameof(PartyId), "PartyId")]
    public partial class PartyReportPage : ContentPage
    {
        private int _partyId;

        // આ Property MAUI Shell Call કરશે
        public string PartyId
        {
            set
            {
                if (int.TryParse(value, out int id))
                {
                    _partyId = id;
                    LoadPartyReport(id);
                }
            }
        }

        public PartyReportPage()
        {
            InitializeComponent();
        }

        private async void LoadPartyReport(int partyId)
        {
            // અહીં તમારો Report Load કરવાનો Code લખો
            // Example: 
            // var party = await Database.GetPartyAsync(partyId);
            // PartyNameLabel.Text = party.Name;

            System.Diagnostics.Debug.WriteLine($"Party Report Loading for ID: {partyId}");
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

