using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class DeletedHistoryPage : ContentPage
    {
        private DeletedHistoryService _service = new DeletedHistoryService();

        public DeletedHistoryPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            var list = await _service.GetHistoryAsync();
            HistoryList.ItemsSource = list;
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int id = (int)btn.CommandParameter;

            bool ans = await DisplayAlert("♻️ Restore?", "આ Data પાછો લાવવો છે?\nDelete History માંથી નીકળી જશે અને Main List માં આવી જશે!", "હા, પાછો લાવો", "ના");
            if (ans)
            {
                bool ok = await _service.RestoreAsync(id);
                if (ok)
                {
                    await DisplayAlert("✅ Done", "Data Restore થઈ ગયો!", "OK");
                    await LoadData();
                }
                else
                {
                    await DisplayAlert("Error", "Restore નથી થયું", "OK");
                }
            }
        }

        private async void OnPermanentDeleteClicked(object sender, EventArgs e)
        {
            var btn = sender as Button;
            int id = (int)btn.CommandParameter;

            bool ans = await DisplayAlert("❌ કાયમ માટે Delete?", "આ Data કાયમ માટે Delete થઈ જશે!\nપછી પાછો નહીં આવે!", "હા, Delete કરો", "ના");
            if (ans)
            {
                await _service.DeletePermanentAsync(id);
                await LoadData();
            }
        }

        private async void OnClearAllClicked(object sender, EventArgs e)
        {
            bool ans = await DisplayAlert("Clear All?", "બધો Deleted History Clear કરવો છે?", "હા", "ના");
            if (ans)
            {
                await _service.ClearAllAsync();
                await LoadData();
                await DisplayAlert("Cleared", "બધો History Clear થઈ ગયો!", "OK");
            }
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

