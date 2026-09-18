using ARSoftware.Models; using ARSoftware.Services; namespace ARSoftware.Pages { public partial class ItemMasterPage : ContentPage { private SupabaseService _supabase; private Item? _editingItem = null; public ItemMasterPage(SupabaseService supabase) { InitializeComponent(); _supabase = supabase; } protected override async void OnAppearing() { base.OnAppearing(); await LoadItems(); } private async Task LoadItems() { var items = await _supabase.GetItems(); ItemsCollection.ItemsSource = items; } private async void OnSaveClicked(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(ItemNameEntry.Text)) { await DisplayAlert("Error", "Item Name જરૂરી છે!", "OK"); return; } if (_editingItem == null) { var item = new Item { Name = ItemNameEntry.Text, RatePerKg = decimal.TryParse(RateEntry.Text, out var r) ? r : 0 }; await _supabase.AddItem(item); } else { _editingItem.Name = ItemNameEntry.Text; _editingItem.RatePerKg = decimal.TryParse(RateEntry.Text, out var r2) ? r2 : 0; await _supabase.UpdateItem(_editingItem); } ClearForm(); await LoadItems(); } private void OnCancelClicked(object sender, EventArgs e) => ClearForm(); private void ClearForm() { ItemNameEntry.Text = RateEntry.Text = ""; _editingItem = null; FormTitleLabel.Text = "New Item"; SaveButton.Text = "Save"; CancelButton.IsVisible = false; } private void OnEditClicked(object sender, EventArgs e) { if (sender is Button btn && btn.BindingContext is Item item) { _editingItem = item; ItemNameEntry.Text = item.Name; RateEntry.Text = item.RatePerKg.ToString(); FormTitleLabel.Text = "Edit Item - " + item.Name; SaveButton.Text = "Update"; CancelButton.IsVisible = true; } } private async void OnDeleteClicked(object sender, EventArgs e) { if (sender is Button btn && btn.BindingContext is Item item) { bool confirm = await DisplayAlert("Delete?", $"{item.Name} Delete?", "Yes", "Cancel"); if (confirm) { await _supabase.DeleteItem(item.Id); await LoadItems(); } } } 
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

