using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class JavakEntryPage : ContentPage
    {
        private SupabaseService _service;
        private JobworkEntry? _selected;

        private PrintService _printService = new PrintService(); // આ Field Add કરો Class ની અંદર Top માં

        public JavakEntryPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
            JavakDatePicker.Date = DateTime.Now.Date;
            DeliveredBagsEntry.TextChanged += OnDeliveredChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPending();
            // Form Hide Rakho - New Entry Click Karo To Khulse - Baki Hide Rahese
            JavakForm.IsVisible = false;
            NewJavakButtonBorder.IsVisible = true;
        }

        private void OnNewJavakClicked(object sender, EventArgs e)
        {
            // New Javak Entry - Pending List maathi Party Select Karo
            DisplayAlert("Javak", "Pending Stock List maathi Party Select Karo! Party par Click Karo To Entry Khulse - Form Auto Khulse - Baki Hide Rahese - Badhi Detail Auto Aavse!", "OK");
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Cancel / Hide - Form Hide Karo - Button Show Karo
            JavakForm.IsVisible = false;
            NewJavakButtonBorder.IsVisible = true;
            _selected = null;
            DeliveredBagsEntry.Text = JavakVehicleEntry.Text = PartyMobileEntry.Text = "";
            BillNoEntry.Text = PartyNameEntry.Text = ItemNameEntry.Text = JobworkTypeEntry.Text = "";
            TotalWeightEntry.Text = NetWeightEntry.Text = AmountEntry.Text = "";
            JavakDatePicker.Date = DateTime.Now.Date;
        }

        private async Task LoadPending()
        {
            var all = await _service.GetEntries();
            var pendingList = all.Where(e => e.Status == "Pending" || e.Status == "Partial Delivered").ToList();
            PendingCollection.ItemsSource = pendingList;
        }

        private void OnPendingSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is JobworkEntry entry)
            {
                _selected = entry;

                BillNoEntry.Text = entry.BillNo;
                PartyNameEntry.Text = entry.PartyName;
                ItemNameEntry.Text = entry.ItemName;
                JobworkTypeEntry.Text = "Grinding";
                DeliveredBagsEntry.Text = "";
                BagWeightEntry.Text = entry.BagWeight.ToString();
                TotalWeightEntry.Text = entry.TotalWeight.ToString("N0");
                VehicleWeightEntry.Text = entry.BridgeWeight.ToString("N0");
                KharaboEntry.Text = entry.Kharabo.ToString("N0");
                NetWeightEntry.Text = entry.NetWeight.ToString("N0");
                RateEntry.Text = entry.RatePerKg.ToString();
                AmountEntry.Text = entry.TotalAmount.ToString("N0");

                if (entry.Status == "Partial Delivered")
                {
                    AutoBillLabel.Text = $"⚠️ BAKI STOCK: {entry.PendingBags} Bags Baki Che! Total: {entry.Bags}, Delivered: {entry.DeliveredBags}, BAKI: {entry.PendingBags}";
                    CalculationLabel.Text = $"Party: {entry.PartyName} - Total: {entry.Bags} | Delivered: {entry.DeliveredBags} | BAKI: {entry.PendingBags} Bags Pending";
                }
                else
                {
                    AutoBillLabel.Text = $"Total Stock: {entry.Bags} Bags - Bill: Rs {entry.TotalAmount:N0}";
                    CalculationLabel.Text = $"{entry.Bags} Bags x {entry.BagWeight} KG = {entry.TotalWeight} KG - {entry.Kharabo} Kharabo = {entry.NetWeight} Net x Rs {entry.RatePerKg}";
                }

                // Form Show Karo - Button Hide Karo - Party Click Karo To Entry Khulse
                JavakForm.IsVisible = true;
                NewJavakButtonBorder.IsVisible = false;
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnDeliveredChanged(object sender, TextChangedEventArgs e)
        {
            if (_selected != null && int.TryParse(DeliveredBagsEntry.Text, out var delivered))
            {
                int pendingBags = _selected.PendingBags > 0 ? _selected.PendingBags : _selected.Bags;

                if (delivered > pendingBags)
                {
                    AutoBillLabel.Text = $"❌ ERROR: {pendingBags} Bags જ Baki Che! Baki: {pendingBags} Bags";
                    return;
                }

                int bakiAfterDelivery = pendingBags - delivered;
                decimal perBagAmount = _selected.TotalAmount / _selected.Bags;
                decimal perBagWeight = _selected.NetWeight / _selected.Bags;
                decimal newAmount = perBagAmount * delivered;
                decimal newWeight = perBagWeight * delivered;
                decimal bakiWeight = perBagWeight * bakiAfterDelivery;
                decimal bakiAmount = perBagAmount * bakiAfterDelivery;

                TotalWeightEntry.Text = ((_selected.TotalWeight / _selected.Bags) * delivered).ToString("N0");
                NetWeightEntry.Text = newWeight.ToString("N0");
                AmountEntry.Text = newAmount.ToString("N0");

                AutoBillLabel.Text = $"✅ Javak: {delivered} Bags | Bill: Rs {newAmount:N0} | BAKI STOCK: {bakiAfterDelivery} Bags ({bakiWeight:N0} KG) Rs {bakiAmount:N0} Baki Rahese!";
                CalculationLabel.Text = $"Javak: {delivered} Bags x {_selected.BagWeight} KG = {TotalWeightEntry.Text} KG | BAKI: {bakiAfterDelivery} Bags ({bakiWeight:N0} KG) Pending!";
            }
        }

        private async void OnJavakSaveClicked(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                await DisplayAlert("Error", "Pending Entry Select Karo!", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(JavakVehicleEntry.Text))
            {
                await DisplayAlert("Error", "Javak Gaadi Number Lakho!", "OK");
                return;
            }

            int delivered = int.TryParse(DeliveredBagsEntry.Text, out var d2) ? d2 : 0;
            int pendingBags = _selected.PendingBags > 0 ? _selected.PendingBags : _selected.Bags;

            if (delivered == 0)
            {
                await DisplayAlert("Error", "Delivered Bags Lakho!", "OK");
                return;
            }
            if (delivered > pendingBags)
            {
                await DisplayAlert("Error", $"Baki: {pendingBags} Bags j che! {delivered} na mokli sakay!", "OK");
                return;
            }

            int totalDeliveredNow = _selected.DeliveredBags + delivered;
            int bakiAfterDelivery = _selected.Bags - totalDeliveredNow;

            _selected.DeliveredBags = totalDeliveredNow;
            _selected.PendingBags = bakiAfterDelivery;
            _selected.Status = bakiAfterDelivery == 0 ? "Delivered" : "Partial Delivered";
            string javakVehicle = JavakVehicleEntry.Text;
            _selected.VehicleNo = $"{_selected.VehicleNo} -> JAVAK: {javakVehicle} ({JavakDatePicker.Date:dd/MM/yyyy}) - {delivered} Bags";

            await _service.UpdateEntry(_selected);

            decimal billAmount = decimal.TryParse(AmountEntry.Text, out var amt) ? amt : 0;
            decimal netWeight = decimal.TryParse(NetWeightEntry.Text, out var nw) ? nw : 0;

            string bakiMessage = "";
            if (bakiAfterDelivery > 0)
            {
                decimal perBagWeight = _selected.NetWeight / _selected.Bags * bakiAfterDelivery;
                decimal perBagAmount = _selected.TotalAmount / _selected.Bags * bakiAfterDelivery;
                bakiMessage = $"\n\n📦 BAKI STOCK: {_selected.PartyName} No Baki: {bakiAfterDelivery} Bags ({perBagWeight:N0} KG) Rs {perBagAmount:N0} Baki!";
            }
            else
            {
                bakiMessage = $"\n\n✅ FULL DELIVERED! Baki: 0 Bags";
            }

            await DisplayAlert("✅ Javak Success!",
                $"Party: {_selected.PartyName}\nBill: {_selected.BillNo}\nAavak: {_selected.Bags} Total\nJavak: {delivered} Bags\nTotal Delivered: {totalDeliveredNow}\nBAKI: {bakiAfterDelivery} Bags\nVehicle: {javakVehicle}\nAmount: Rs {billAmount:N0}{bakiMessage}\n\nPDF + WhatsApp!", "OK");

            await PrintService.PrintBillPDF(_selected.BillNo, _selected.PartyName, _selected.ItemName, "Grinding", delivered, _selected.BagWeight, decimal.Parse(TotalWeightEntry.Text.Replace(",", "")), _selected.BridgeWeight, _selected.Kharabo, netWeight, _selected.RatePerKg, billAmount, javakVehicle, _selected.Status, _selected.EntryDate, JavakDatePicker.Date);

            if (!string.IsNullOrWhiteSpace(PartyMobileEntry.Text))
            {
                await WhatsAppService.SendDeliveryMessage(_selected.PartyName, PartyMobileEntry.Text, _selected.BillNo, _selected.ItemName, delivered, netWeight, javakVehicle, JavakDatePicker.Date, billAmount);
            }

            // Form Hide Karo - Button Show Karo - Baki Hide Rahese
            JavakForm.IsVisible = false;
            NewJavakButtonBorder.IsVisible = true;
            _selected = null;
            DeliveredBagsEntry.Text = JavakVehicleEntry.Text = PartyMobileEntry.Text = "";
            BillNoEntry.Text = PartyNameEntry.Text = ItemNameEntry.Text = "";
            JavakDatePicker.Date = DateTime.Now.Date;
            await LoadPending();
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

