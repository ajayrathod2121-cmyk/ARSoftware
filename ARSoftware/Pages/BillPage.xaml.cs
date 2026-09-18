using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class BillPage : ContentPage
    {
        private SupabaseService _service;
        private List<dynamic> _billItems = new();
        private string _billType = "Jobwork";
        private Party _selectedParty;

        public BillPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
            LoadCompanyDetails();
            LoadBillSettings();
            BillNoEntry.Text = $"BILL-{DateTime.Now:yyMMddHHmm}";
            BillDatePicker.Date = DateTime.Now;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCompanyDetails(); // ✅ Settings માંથી પાછા આવો ત્યારે Refresh
        }

        private void LoadCompanyDetails()
        {
            // ✅ 14 DETAIL બધી બિલ માં આવશે - Text Black
            string name = Preferences.Get("BusinessName", "AR SOFTWARE");
            string address = Preferences.Get("BusinessAddress", "Mahuva, Gujarat");
            string city = Preferences.Get("BusinessCity", "Bhavnagar");
            string pincode = Preferences.Get("BusinessPincode", "364001");
            string mobile = Preferences.Get("BusinessMobile", "9876543210");
            string email = Preferences.Get("BusinessEmail", "");
            string gst = Preferences.Get("BusinessGst", "");
            string bankName = Preferences.Get("BankName", "SBI");
            string accNo = Preferences.Get("AccountNo", "");
            string ifsc = Preferences.Get("IfscCode", "");
            string holder = Preferences.Get("AccountHolder", "");
            string upi = Preferences.Get("UpiId", "");
            string terms = Preferences.Get("BillTerms", "1. Payment Due in 7 Days\n2. Goods once sold will not be taken back");

            CompanyNameLabel.Text = name;
            CompanyAddressLabel.Text = $"{address}, {city} - {pincode}";
            CompanyMobileLabel.Text = $"📞 {mobile} | {email} | GST: {gst}";

            // ✅ Bank Detail Bill માં
            BillBankLabel.Text = $"Bank: {bankName} ({holder})";
            BillAccountLabel.Text = $"A/C No: {accNo}";
            BillIfscLabel.Text = $"IFSC: {ifsc}";
            BillUpiLabel.Text = $"UPI: {upi}";
            BillTermsLabel.Text = terms;

            // ✅ Logo + QR
            string logoPath = Preferences.Get("BusinessLogoPath", "");
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                LogoImage.Source = ImageSource.FromFile(logoPath);
                LogoImage.IsVisible = true; LogoText.IsVisible = false;
            }
            string qrPath = Preferences.Get("UpiQrPath", "");
            if (!string.IsNullOrEmpty(qrPath) && File.Exists(qrPath))
            {
                BillQrImage.Source = ImageSource.FromFile(qrPath);
                BillQrImage.IsVisible = true; BillQrText.IsVisible = false;
            }
        }

        private void LoadBillSettings()
        {
            _billType = Preferences.Get("BillType", "Jobwork");
            UpdateBillTypeUI(_billType);
            UpdateTotal();
        }

        private void UpdateBillTypeUI(string type)
        {
            JobworkBorder.BackgroundColor = Color.FromArgb("#1A1A1A");
            GstBorder.BackgroundColor = Color.FromArgb("#1A1A1A");
            RegularBorder.BackgroundColor = Color.FromArgb("#1A1A1A");
            if (type == "Jobwork") JobworkBorder.BackgroundColor = Color.FromArgb("#FF6B00");
            else if (type == "GST") GstBorder.BackgroundColor = Color.FromArgb("#FF6B00");
            else RegularBorder.BackgroundColor = Color.FromArgb("#FF6B00");
            GstGrid.IsVisible = (type == "GST");
            UpdateTotal();
        }

        private void OnBillTypeClicked(object sender, TappedEventArgs e)
        {
            var type = "Jobwork";
            if (sender is Border b && b.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tg)
                type = tg.CommandParameter as string ?? type;
            _billType = type; Preferences.Set("BillType", _billType);
            UpdateBillTypeUI(_billType);
        }

        private async void OnSelectPartyClicked(object sender, EventArgs e)
        {
            var parties = await _service.GetParties();
            string[] names = parties.Select(p => p.Name).ToArray();
            string selected = await DisplayActionSheet("Select Party", "Cancel", null, names);
            if (selected != null && selected != "Cancel")
            {
                _selectedParty = parties.FirstOrDefault(p => p.Name == selected);
                PartyEntry.Text = _selectedParty.Name;
                PartyDetailLabel.Text = $"{_selectedParty.Mobile} | {_selectedParty.City} | {_selectedParty.Address}";
                PartyDetailLabel.IsVisible = true;
            }
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            string item = await DisplayPromptAsync("Item", "Item Name:", "Next", "Cancel");
            if (string.IsNullOrWhiteSpace(item)) return;
            string qtyStr = await DisplayPromptAsync("Qty", "Bags:", "Next", "Cancel", "1", -1, Keyboard.Numeric);
            if (!decimal.TryParse(qtyStr, out var qty)) qty = 1;
            string rateStr = await DisplayPromptAsync("Rate", "Rate per Bag:", "Add", "Cancel", "100", -1, Keyboard.Numeric);
            if (!decimal.TryParse(rateStr, out var rate)) rate = 100;
            var billItem = new { ItemName = item, Qty = qty, Rate = rate, Amount = qty * rate };
            _billItems.Add(billItem);
            BillItemsCollection.ItemsSource = null; BillItemsCollection.ItemsSource = _billItems;
            UpdateTotal();
        }

        private void OnRemoveItemClicked(object sender, TappedEventArgs e)
        {
            var item = e.Parameter; if (item != null) _billItems.Remove(item);
            BillItemsCollection.ItemsSource = null; BillItemsCollection.ItemsSource = _billItems; UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal subTotal = 0;
            foreach (var x in _billItems) subTotal += (decimal)x.GetType().GetProperty("Amount").GetValue(x);
            double gstPercentDouble = Preferences.Get("BillGstPercent", 18.0);
            decimal gstPercent = (decimal)gstPercentDouble;
            decimal gstAmt = (_billType == "GST") ? subTotal * gstPercent / 100 : 0;
            decimal grand = subTotal + gstAmt;
            SubTotalLabel.Text = $"Rs {subTotal:N0}"; GstLabel.Text = $"Rs {gstAmt:N0}"; GrandTotalLabel.Text = $"Rs {grand:N0}";
        }

        private async void OnBillSettingsClicked(object sender, EventArgs e)
        {
            double currentGst = Preferences.Get("BillGstPercent", 18.0);
            string gstStr = await DisplayPromptAsync("Bill Setting", "GST %:", "Save", "Cancel", currentGst.ToString(), -1, Keyboard.Numeric);
            if (double.TryParse(gstStr, out var gst)) Preferences.Set("BillGstPercent", gst);
            string terms = await DisplayPromptAsync("Bill Setting", "Terms & Condition:", "Save", "Cancel", Preferences.Get("BillTerms", "Payment Due in 7 Days"));
            if (!string.IsNullOrWhiteSpace(terms)) Preferences.Set("BillTerms", terms);
            await DisplayAlert("✅ Saved", $"GST: {Preferences.Get("BillGstPercent", 18.0)}%\nTerms: {Preferences.Get("BillTerms", "")}", "OK");
            LoadCompanyDetails(); UpdateTotal();
        }

        private async void OnSaveBillClicked(object sender, EventArgs e)
        {
            if (_selectedParty == null) { await DisplayAlert("Party", "પહેલા Party Select કરો!", "OK"); return; }
            if (_billItems.Count == 0) { await DisplayAlert("Item", "Item Add કરો!", "OK"); return; }
            await DisplayAlert("✅ Bill Saved", $"{CompanyNameLabel.Text}\nBill: {BillNoEntry.Text}\nParty: {_selectedParty.Name}\nTotal: {GrandTotalLabel.Text}\nBank: {BillBankLabel.Text}\nUPI: {BillUpiLabel.Text}", "OK");
        }

        private async void OnWhatsAppBillClicked(object sender, EventArgs e)
        {
            if (_selectedParty == null) return;
            string msg = $"*{CompanyNameLabel.Text}*\n{CompanyAddressLabel.Text}\n{CompanyMobileLabel.Text}\n\nBill: {BillNoEntry.Text}\nParty: {_selectedParty.Name}\nTotal: {GrandTotalLabel.Text}\n\nBank: {BillBankLabel.Text}\n{BillIfscLabel.Text}\nUPI: {BillUpiLabel.Text}\n\n{BillTermsLabel.Text}\n\n- {CompanyNameLabel.Text}";
            await Launcher.OpenAsync($"https://wa.me/91{_selectedParty.Mobile}?text={Uri.EscapeDataString(msg)}");
        }

        private async void OnPrintBillClicked(object sender, EventArgs e)
        {
            try { await DisplayAlert("🖨 Print", $"Premium Bill PDF - {_billType}\n{CompanyNameLabel.Text}\n{CompanyAddressLabel.Text}\n{CompanyMobileLabel.Text}\n\n{BillBankLabel.Text}\n{BillAccountLabel.Text}\n{BillIfscLabel.Text}\n{BillUpiLabel.Text}\n\nTotal: {GrandTotalLabel.Text}", "Print"); }
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

