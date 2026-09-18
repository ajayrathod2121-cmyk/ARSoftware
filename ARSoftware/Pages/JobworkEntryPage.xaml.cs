using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    [QueryProperty(nameof(EntryId), "Id")]
    public partial class JobworkEntryPage : ContentPage
    {
        private SupabaseService _service;
        private JobworkEntry? _editingEntry = null;
        private List<string> _savedJobworkTypes = new List<string>();

        public string EntryId
        {
            set
            {
                if (int.TryParse(value, out int id))
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var all = await _service.GetEntries();
                        var ent = all.FirstOrDefault(x => x.Id == id);
                        if (ent != null) LoadEntryForEdit(ent);
                    });
                }
            }
        }

        public JobworkEntryPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
            EntryDatePicker.Date = DateTime.Now.Date;
            BillNoEntry.Text = $"B{DateTime.Now:yyyyMMddHHmm}_{new Random().Next(10, 99)}";
            BagsEntry.TextChanged += OnCalc;
            BagWeightEntry.TextChanged += OnCalc;
            VehicleWeightEntry.TextChanged += OnCalc;
            KharaboEntry.TextChanged += OnCalc;
            RateEntry.TextChanged += OnCalc;
            LoadSavedJobworkTypes();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAllBills();
            AavakForm.IsVisible = false;
            NewEntryButtonBorder.IsVisible = true;
        }

        private void LoadSavedJobworkTypes()
        {
            string saved = Preferences.Get("SavedJobworkTypes", "Grinding,Cleaning,Sorting,Packing,Roasting,Mixing");
            _savedJobworkTypes = saved.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            try { var cv = this.FindByName<CollectionView>("SavedJobworkTypesCollection"); if (cv != null) cv.ItemsSource = _savedJobworkTypes; } catch { }
        }

        private void OnSavedJobworkSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is string jobwork)
            {
                JobworkTypeEntry.Text = jobwork;
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnNewEntryClicked(object sender, EventArgs e)
        {
            _editingEntry = null;
            BillNoEntry.Text = $"B{DateTime.Now:yyyyMMddHHmm}_{new Random().Next(10, 99)}";
            PartyNameEntry.Text = ItemNameEntry.Text = JobworkTypeEntry.Text = BagsEntry.Text = VehicleEntry.Text = "";
            VehicleWeightEntry.Text = "0";
            KharaboEntry.Text = "0";
            TotalWeightEntry.Text = NetWeightEntry.Text = AmountEntry.Text = "";
            CalculationLabel.Text = "Bags x Bag Weight = Total - Kharabo = Net x Rate = Amount";
            EntryDatePicker.Date = DateTime.Now.Date;
            SaveButton.Text = "💾 Save Aavak";
            SaveButton.BackgroundColor = Color.FromArgb("#FF6B00");
            CancelButton.IsVisible = true;
            AavakForm.IsVisible = true;
            NewEntryButtonBorder.IsVisible = false;
        }

        private async Task LoadAllBills()
        {
            var all = await _service.GetEntries();
            AllBillsCollection.ItemsSource = all.OrderByDescending(x => x.Id).ToList();
        }

        private void OnCalc(object sender, TextChangedEventArgs e)
        {
            try
            {
                int bags = int.TryParse(BagsEntry.Text, out var b) ? b : 0;
                decimal bagW = decimal.TryParse(BagWeightEntry.Text, out var bw) ? bw : 0;
                decimal vehW = decimal.TryParse(VehicleWeightEntry.Text, out var vw) ? vw : 0;
                decimal kharabo = decimal.TryParse(KharaboEntry.Text, out var k) ? k : 0;
                decimal rate = decimal.TryParse(RateEntry.Text, out var r) ? r : 0;
                decimal total = bags * bagW;
                if (vehW > 0) total = vehW;
                decimal net = total - kharabo;
                decimal amt = net * rate;
                TotalWeightEntry.Text = total.ToString("N0");
                NetWeightEntry.Text = net.ToString("N0");
                AmountEntry.Text = amt.ToString("N0");
                CalculationLabel.Text = $"{bags} Bags x {bagW} KG = {total} KG - {kharabo} Kharabo = {net} Net x Rs {rate} = Rs {amt:N0}";
            }
            catch { }
        }

        private void OnBillSelectedForEdit(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is JobworkEntry entry)
            {
                LoadEntryForEdit(entry);
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnEditBillClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
            {
                LoadEntryForEdit(entry);
            }
        }

        private void LoadEntryForEdit(JobworkEntry entry)
        {
            _editingEntry = entry;
            BillNoEntry.Text = entry.BillNo;
            PartyNameEntry.Text = entry.PartyName;
            ItemNameEntry.Text = entry.ItemName;
            JobworkTypeEntry.Text = "Grinding";
            BagsEntry.Text = entry.Bags.ToString();
            BagWeightEntry.Text = entry.BagWeight.ToString();
            TotalWeightEntry.Text = entry.TotalWeight.ToString("N0");
            VehicleWeightEntry.Text = entry.BridgeWeight.ToString();
            KharaboEntry.Text = entry.Kharabo.ToString();
            NetWeightEntry.Text = entry.NetWeight.ToString("N0");
            VehicleEntry.Text = entry.VehicleNo;
            RateEntry.Text = entry.RatePerKg.ToString();
            AmountEntry.Text = entry.TotalAmount.ToString("N0");
            // ✅ FIX 1: DateTime? -> DateTime
            // LoadEntryForEdit માં ફક્ત આ જ રાખો
            EntryDatePicker.Date = entry.EntryDate is DateTime d1 ? d1 : DateTime.Now;
            SaveButton.Text = $"✏ Update Bill {entry.BillNo}";
            SaveButton.BackgroundColor = Color.FromArgb("#0066FF");
            CancelButton.IsVisible = true;
            AavakForm.IsVisible = true;
            NewEntryButtonBorder.IsVisible = false;
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            _editingEntry = null;
            BillNoEntry.Text = $"B{DateTime.Now:yyyyMMddHHmm}_{new Random().Next(10, 99)}";
            PartyNameEntry.Text = ItemNameEntry.Text = JobworkTypeEntry.Text = BagsEntry.Text = VehicleEntry.Text = "";
            VehicleWeightEntry.Text = "0";
            KharaboEntry.Text = "0";
            TotalWeightEntry.Text = NetWeightEntry.Text = AmountEntry.Text = "";
            EntryDatePicker.Date = DateTime.Now.Date;
            SaveButton.Text = "💾 Save Aavak";
            SaveButton.BackgroundColor = Color.FromArgb("#FF6B00");
            AavakForm.IsVisible = false;
            NewEntryButtonBorder.IsVisible = true;
        }

        private async void OnDeleteBillClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is JobworkEntry entry)
            {
                // ✅ FIX 2: DisplayAlert -> DisplayAlert
                bool confirm = await DisplayAlert("Delete?", $"{entry.BillNo} - {entry.PartyName} - {entry.Bags} Bags Delete?", "Yes Delete", "Cancel");
                if (confirm)
                {
                    await _service.DeleteEntry(entry.Id);
                    await LoadAllBills();
                }
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PartyNameEntry.Text) || string.IsNullOrWhiteSpace(BagsEntry.Text) || string.IsNullOrWhiteSpace(VehicleEntry.Text))
            {
                await DisplayAlert("Error", "Party + Bags + Vehicle No required!", "OK");
                return;
            }

            int bags = int.TryParse(BagsEntry.Text, out var b2) ? b2 : 0;
            decimal bagW = decimal.TryParse(BagWeightEntry.Text, out var bw2) ? bw2 : 40;
            decimal vehW = decimal.TryParse(VehicleWeightEntry.Text, out var vw2) ? vw2 : 0;
            decimal total = bags * bagW;
            if (vehW > 0) total = vehW;
            decimal kharabo = decimal.TryParse(KharaboEntry.Text, out var k2) ? k2 : 0;
            decimal net = total - kharabo;
            decimal rate = decimal.TryParse(RateEntry.Text, out var r2) ? r2 : 50;
            decimal amt = net * rate;

            string jobworkType = JobworkTypeEntry.Text?.Trim() ?? "Grinding";
            if (!string.IsNullOrWhiteSpace(jobworkType) && !_savedJobworkTypes.Contains(jobworkType))
            {
                _savedJobworkTypes.Add(jobworkType);
                Preferences.Set("SavedJobworkTypes", string.Join(",", _savedJobworkTypes));
                try { var cv = this.FindByName<CollectionView>("SavedJobworkTypesCollection"); if (cv != null) { cv.ItemsSource = null; cv.ItemsSource = _savedJobworkTypes; } } catch { }
            }

            if (_editingEntry == null)
            {
                var entry = new JobworkEntry
                {
                    BillNo = BillNoEntry.Text,
                    PartyName = PartyNameEntry.Text,
                    ItemName = ItemNameEntry.Text,
                    Bags = bags,
                    BagWeight = bagW,
                    TotalWeight = total,
                    BridgeWeight = vehW,
                    Kharabo = kharabo,
                    NetWeight = net,
                    RatePerKg = rate,
                    TotalAmount = amt,
                    VehicleNo = VehicleEntry.Text,
                    Status = "Pending",
                    PendingBags = bags,
                    // ✅ FIX 3: DatePicker.Date હવે DateTime છે - Nullable ને Handle કર્યું
                    EntryDate = EntryDatePicker.Date,
                    CreatedAt = DateTime.Now
                };
                await _service.AddEntry(entry);
                await DisplayAlert("✅ Saved!", $"Bill: {entry.BillNo} Party: {entry.PartyName} Jobwork: {jobworkType}", "OK");
                await PrintService.PrintBillPDF(entry);
            }
            else
            {
                _editingEntry.BillNo = BillNoEntry.Text;
                _editingEntry.PartyName = PartyNameEntry.Text;
                _editingEntry.ItemName = ItemNameEntry.Text;
                _editingEntry.Bags = bags;
                _editingEntry.BagWeight = bagW;
                _editingEntry.TotalWeight = total;
                _editingEntry.BridgeWeight = vehW;
                _editingEntry.Kharabo = kharabo;
                _editingEntry.NetWeight = net;
                _editingEntry.RatePerKg = rate;
                _editingEntry.TotalAmount = amt;
                _editingEntry.VehicleNo = VehicleEntry.Text;
                _editingEntry.EntryDate = EntryDatePicker.Date; // ✅ FIX 5
                _editingEntry.PendingBags = _editingEntry.Bags - _editingEntry.DeliveredBags;
                await _service.UpdateEntry(_editingEntry);
                await DisplayAlert("✅ Updated!", $"Bill {_editingEntry.BillNo} Updated!", "OK");
            }

            _editingEntry = null;
            BillNoEntry.Text = $"B{DateTime.Now:yyyyMMddHHmm}_{new Random().Next(10, 99)}";
            PartyNameEntry.Text = ItemNameEntry.Text = JobworkTypeEntry.Text = BagsEntry.Text = VehicleEntry.Text = "";
            VehicleWeightEntry.Text = "0"; KharaboEntry.Text = "0";
            TotalWeightEntry.Text = NetWeightEntry.Text = AmountEntry.Text = "";
            EntryDatePicker.Date = DateTime.Now.Date;
            SaveButton.Text = "💾 Save Aavak";
            SaveButton.BackgroundColor = Color.FromArgb("#FF6B00");
            AavakForm.IsVisible = false;
            NewEntryButtonBorder.IsVisible = true;
            await LoadAllBills();
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

