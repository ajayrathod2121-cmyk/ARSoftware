using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class PartyMasterPage : ContentPage
    {
        private SupabaseService _service;
        private List<Party> _parties = new();
        private List<JobworkEntry> _entries = new();
        private List<Payment> _payments = new();

        public PartyMasterPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _parties = await _service.GetParties();
                _entries = await _service.GetEntries();
                _payments = await _service.GetPayments();
                BindParties(_parties);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void BindParties(List<Party> parties)
        {
            var partyDetails = parties.Select(p =>
            {
                var totalBags = _entries.Where(e => e.PartyName.ToLower() == p.Name.ToLower() || e.PartyId == p.Id).Sum(e => e.Bags);
                var totalBilling = _entries.Where(e => e.PartyName.ToLower() == p.Name.ToLower() || e.PartyId == p.Id).Sum(e => e.TotalAmount);
                var totalPaid = _payments.Where(x => x.PartyName.ToLower() == p.Name.ToLower() || x.PartyId == p.Id).Sum(x => x.Amount);
                var baki = totalBilling - totalPaid;
                return new
                {
                    Party = p,
                    Id = p.Id,
                    Name = p.Name,
                    Mobile = p.Mobile,
                    City = p.City,
                    Address = p.Address,
                    TotalBags = totalBags,
                    Baki = baki,
                };
            }).OrderByDescending(x => x.Baki).ToList();

            PartyCollection.ItemsSource = partyDetails;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = e.NewTextValue?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(txt)) BindParties(_parties);
            else
            {
                var filtered = _parties.Where(p => p.Name.ToLower().Contains(txt) || p.City.ToLower().Contains(txt) || p.Mobile.Contains(txt)).ToList();
                BindParties(filtered);
            }
        }

        private Party GetPartyFromSender(object sender)
        {
            try
            {
                var bindable = sender as BindableObject;
                var ctx = bindable?.BindingContext;
                if (ctx == null) return null;
                if (ctx is Party directParty) return directParty;
                var prop = ctx.GetType().GetProperty("Party");
                if (prop != null)
                {
                    return prop.GetValue(ctx) as Party;
                }
            }
            catch { }
            return null;
        }

        private async void OnAddPartyClicked(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Add Party", "Party Name:", "Next", "Cancel", "", 50);
            if (string.IsNullOrWhiteSpace(name)) return;
            string mobile = await DisplayPromptAsync("Mobile", $"{name} - Mobile No:", "Next", "Cancel", "", 10, Keyboard.Telephone);
            string city = await DisplayPromptAsync("City", $"{name} - City:", "Next", "Cancel", "bhavnagar", 30);
            string address = await DisplayPromptAsync("Address", $"{name} - Address:", "Save", "Cancel", "", 100);

            var party = new Party { Name = name.Trim(), Mobile = mobile?.Trim() ?? "", City = city?.Trim() ?? "Bhavnagar", Address = address?.Trim() ?? "", CreatedAt = DateTime.Now };
            await _service.AddParty(party);
            await LoadData();
        }

        private async void OnCallClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p != null && !string.IsNullOrWhiteSpace(p.Mobile))
            {
                try { PhoneDialer.Default.Open(p.Mobile); } catch { await DisplayAlert("Call", p.Mobile, "OK"); }
            }
            else await DisplayAlert("No Mobile", "Mobile Number Not Available!", "OK");
        }

        private async void OnWhatsappClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p != null && !string.IsNullOrWhiteSpace(p.Mobile))
            {
                decimal baki = 0;
                try { baki = await _service.GetPartyBaki(p.Id); } catch { }
                var msg = $"Hello {p.Name},\nYour Baki is Rs {baki:N0}/-\n- AR Software";
                await Launcher.OpenAsync($"https://wa.me/91{p.Mobile}?text={Uri.EscapeDataString(msg)}");
            }
        }

        private async void OnEditPartyClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p == null) return;
            string name = await DisplayPromptAsync("Edit Name", "Name:", "Next", "Cancel", p.Name, 50);
            if (string.IsNullOrWhiteSpace(name)) return;
            string mobile = await DisplayPromptAsync("Edit Mobile", "Mobile:", "Next", "Cancel", p.Mobile, 10, Keyboard.Telephone);
            string city = await DisplayPromptAsync("Edit City", "City:", "Next", "Cancel", p.City, 30);
            string address = await DisplayPromptAsync("Edit Address", "Address:", "Save", "Cancel", p.Address, 100);

            p.Name = name.Trim();
            p.Mobile = mobile?.Trim() ?? "";
            p.City = city?.Trim() ?? "Bhavnagar";
            p.Address = address?.Trim() ?? "";
            await _service.UpdateParty(p);
            await LoadData();
        }

        private async void OnDeletePartyClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p == null) return;
            if (await DisplayAlert("Delete?", $"{p.Name} - Delete?", "Yes", "Cancel"))
            {
                await _service.DeleteParty(p.Id);
                await LoadData();
            }
        }

        // ✅ FIXED 100% - અહીં જ ભૂલ હતી!
        private async void OnHistoryClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p == null) return;
            // કોઈ /// નહીં, કોઈ / નહીં - સીધું નામ!
            await Shell.Current.GoToAsync($"PartyReportPage?PartyId={p.Id}");
        }

        // ✅ FIXED 100% - અહીં પણ!
        private async void OnPaymentClicked(object sender, TappedEventArgs e)
        {
            var p = GetPartyFromSender(sender);
            if (p == null) return;
            await Shell.Current.GoToAsync($"PartyReportPage?PartyId={p.Id}");
        }

        private async void OnPrintPdfClicked(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintDetailedStockReport(_entries);
                await DisplayAlert("PDF", $"PDF Generated!\n{path}", "Open");
                if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
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

