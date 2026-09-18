using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class BillsPage : ContentPage
    {
        private SupabaseService _service;

        public BillsPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadBills();
        }

        private async Task LoadBills()
        {
            try
            {
                var entries = await _service.GetEntries();
                // FIXED: Direct JobworkEntry List - Anonymous નહીં - XAML Bindings સાચા ચાલશે
                BillsCollection.ItemsSource = entries.OrderByDescending(x => x.Id).ToList();

                // Summary Update
                try
                {
                    var totalBags = entries.Sum(x => x.Bags);
                    var deliveredBags = entries.Sum(x => x.DeliveredBags);
                    var pendingBags = entries.Sum(x => x.PendingBags);
                    var totalBill = entries.Sum(x => x.TotalAmount);
                    var totalBaki = await _service.GetTotalBaki();

                    SummaryLabel.Text = $"Total: {totalBags} Bags | Delivered: {deliveredBags} | Pending: {pendingBags} | Billing: Rs {totalBill:N0} | Baki: Rs {totalBaki:N0}";
                }
                catch { }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnPrintClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
                {
                    var path = await PrintService.PrintBillPDF(entry);
                    await DisplayAlert($"Bill {entry.BillNo}", $"Party: {entry.PartyName}\nTotal: {entry.Bags} Bags\nDelivered: {entry.DeliveredBags} Bags\nPending: {entry.PendingBags} Bags\nBill Amount: Rs {entry.TotalAmount:N0} (માત્ર Delivered નું)\nStatus: {entry.Status}", "Open PDF");
                    if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
                }
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        // FIXED: 1000 માંથી 500 Javak - 500 નું જ Bill
        private async void OnDeliverPartialClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
                {
                    if (entry.PendingBags <= 0)
                    {
                        await DisplayAlert("Done", $"{entry.BillNo} - બધો માલ મોકલાઈ ગયો છે!\nTotal: {entry.Bags} | Delivered: {entry.DeliveredBags}", "OK");
                        return;
                    }

                    string msg = $"{entry.BillNo} - {entry.PartyName}\nTotal: {entry.Bags} Bags\nDelivered: {entry.DeliveredBags} Bags\nPending: {entry.PendingBags} Bags\n\nકેટલી બેગ મોકલવી છે?";
                    string deliverStr = await DisplayPromptAsync("🚚 Javak", msg, "Deliver", "Cancel", entry.PendingBags.ToString(), 5, Keyboard.Numeric);

                    if (!int.TryParse(deliverStr, out int deliverBags)) return;

                    if (deliverBags <= 0 || deliverBags > entry.PendingBags)
                    {
                        await DisplayAlert("Error", $"1 થી {entry.PendingBags} વચ્ચે નાખો!", "OK");
                        return;
                    }

                    await _service.DeliverPartial(entry.Id, deliverBags);
                    await LoadBills();

                    var updated = (await _service.GetEntries()).FirstOrDefault(x => x.Id == entry.Id);
                    if (updated != null)
                        await DisplayAlert("✅ Javak Done", $"{updated.BillNo}\n{deliverBags} Bags Delivered!\n\nTotal: {updated.Bags} Bags\nDelivered: {updated.DeliveredBags} Bags\nPending: {updated.PendingBags} Bags\nBill Amount: Rs {updated.TotalAmount:N0} (માત્ર {updated.DeliveredBags} Bags નું)\nStatus: {updated.Status}", "OK");
                }
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
            {
                bool confirm = await DisplayAlert("Edit Bill?", $"{entry.BillNo} - {entry.PartyName}\nBags: {entry.Bags} - Rs {entry.TotalAmount:N0}\n\nEdit કરવું છે?", "Yes Edit", "Cancel");
                if (confirm)
                    await Shell.Current.GoToAsync($"///{nameof(JobworkEntryPage)}?Id={entry.Id}");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
            {
                if (await DisplayAlert("Delete?", $"{entry.BillNo} - {entry.PartyName}\nTotal: {entry.Bags} Bags\nBill: Rs {entry.TotalAmount:N0}\nDelete?", "Yes", "Cancel"))
                {
                    await _service.DeleteEntry(entry.Id);
                    await LoadBills();
                }
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

