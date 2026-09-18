using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class PaymentPage : ContentPage
    {
        private SupabaseService _service;
        private Payment? _editingPayment = null;
        private bool _isFormOpen = false;

        public PaymentPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
            PaymentTypePicker.SelectedIndexChanged += OnPaymentTypeChanged;
            PaymentTypePicker.SelectedIndex = 0;
            PaymentDatePicker.Date = DateTime.Now.Date;
            CloseForm();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPayments();
        }

        private void OnPaymentTypeChanged(object sender, EventArgs e)
        {
            var type = PaymentTypePicker.SelectedItem?.ToString() ?? "💵 Cash";
            if (type.Contains("Cash"))
            {
                BankDetailBorder.IsVisible = false;
            }
            else
            {
                BankDetailBorder.IsVisible = true;
                if (type.Contains("Cheque"))
                {
                    TransactionLabel.Text = "🧾 Cheque No / UTR";
                    TransactionIdEntry.Placeholder = "🧾 Cheque No";
                }
                else if (type.Contains("RTGS") || type.Contains("NEFT"))
                {
                    TransactionLabel.Text = $"🔢 {type} No / UTR";
                    TransactionIdEntry.Placeholder = "🔢 Reference No / UTR";
                }
                else
                {
                    TransactionLabel.Text = "🔢 UTR / Transaction ID";
                    TransactionIdEntry.Placeholder = "🔢 UPI Transaction ID";
                }
            }
        }

        private void OnAddToggleClicked(object sender, EventArgs e)
        {
            if (_isFormOpen)
                CloseForm();
            else
                OpenForm();
        }

        private void OpenForm()
        {
            AddFormBorder.IsVisible = true;
            BottomAddButton.IsVisible = false;
            TopAddBtnLabel.Text = "✕ બંધ કરો";
            TopAddBtn.BackgroundColor = Color.FromArgb("#0A0A0A");
            _isFormOpen = true;
        }

        private void CloseForm()
        {
            AddFormBorder.IsVisible = false;
            BottomAddButton.IsVisible = true;
            TopAddBtnLabel.Text = "➕ Add Payment";
            TopAddBtn.BackgroundColor = Color.FromArgb("#FF6B00");
            _isFormOpen = false;
            ClearFormFields();
        }

        private void ClearFormFields()
        {
            _editingPayment = null;
            PartyNameEntry.Text = "";
            AmountEntry.Text = "";
            BankNameEntry.Text = "";
            AccountNoEntry.Text = "";
            TransactionIdEntry.Text = "";
            ChequeNoEntry.Text = "";
            IFSCEntry.Text = "";
            RemarkEntry.Text = "";
            PaymentTypePicker.SelectedIndex = 0;
            PaymentDatePicker.Date = DateTime.Now.Date;
            SaveButtonLabel.Text = "💾 Save Payment";
            BankDetailBorder.IsVisible = false;
        }

        private async Task LoadPayments()
        {
            try
            {
                var payments = await _service.GetPayments();
                PaymentCollection.ItemsSource = payments.OrderByDescending(x => x.Id).ToList();
                var total = payments.Sum(p => p.Amount);
                var totalBaki = await _service.GetTotalBaki();
                TotalLabel.Text = $"💰 Received Rs {total:N0} | Baki Rs {totalBaki:N0}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnSavePaymentClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PartyNameEntry.Text) || string.IsNullOrWhiteSpace(AmountEntry.Text))
            {
                await DisplayAlert("❌ Error", "👥 Party Name + 💰 Amount જરૂરી છે!", "OK");
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("❌ Error", "💰 Amount સાચું નાખો!", "OK");
                return;
            }

            var typeRaw = PaymentTypePicker.SelectedItem?.ToString() ?? "💵 Cash";
            var type = typeRaw.Replace("💵 ", "").Replace("🏦 ", "").Replace("📱 ", "").Replace("🔄 ", "").Replace("💳 ", "").Replace("🧾 ", "").Replace("🏧 ", "").Trim();

            if (_editingPayment == null)
            {
                var payment = new Payment
                {
                    PartyName = PartyNameEntry.Text.Trim(),
                    Amount = amount,
                    PaymentType = type,
                    BankName = BankNameEntry.Text?.Trim() ?? "",
                    AccountNo = AccountNoEntry.Text?.Trim() ?? "",
                    TransactionId = TransactionIdEntry.Text?.Trim() ?? "",
                    ChequeNo = ChequeNoEntry.Text?.Trim() ?? "",
                    IFSC = IFSCEntry.Text?.Trim() ?? "",
                    PaymentDate = PaymentDatePicker.Date, // ✅ FIX 1
                    Remark = RemarkEntry.Text?.Trim() ?? "",
                    CreatedAt = DateTime.Now
                };

                var parties = await _service.GetParties();
                var party = parties.FirstOrDefault(p => p.Name.Trim().ToLower() == payment.PartyName.Trim().ToLower());
                if (party != null) payment.PartyId = party.Id;

                await _service.AddPayment(payment);
                await DisplayAlert("✅ Payment Saved", $"👥 {payment.PartyName}\n💰 Rs {payment.Amount:N0}\n💳 Type: {payment.PaymentType}\n🏦 Bank: {payment.BankName}\n🔢 UTR: {payment.TransactionId}", "OK");
            }
            else
            {
                _editingPayment.PartyName = PartyNameEntry.Text.Trim();
                _editingPayment.Amount = amount;
                _editingPayment.PaymentType = type;
                _editingPayment.BankName = BankNameEntry.Text?.Trim() ?? "";
                _editingPayment.AccountNo = AccountNoEntry.Text?.Trim() ?? "";
                _editingPayment.TransactionId = TransactionIdEntry.Text?.Trim() ?? "";
                _editingPayment.ChequeNo = ChequeNoEntry.Text?.Trim() ?? "";
                _editingPayment.IFSC = IFSCEntry.Text?.Trim() ?? "";
                _editingPayment.PaymentDate = PaymentDatePicker.Date;
                _editingPayment.Remark = RemarkEntry.Text?.Trim() ?? "";

                await _service.UpdatePayment(_editingPayment);
                await DisplayAlert("✅ Updated", $"👥 {_editingPayment.PartyName} - Rs {_editingPayment.Amount:N0} Updated!", "OK");
            }

            CloseForm();
            await LoadPayments();
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void OnEditPaymentClicked(object sender, EventArgs e)
        {
            Payment? p = null;
            if (sender is Border b && b.GestureRecognizers.Count > 0 && e is TappedEventArgs tapped)
            {
                var border = sender as Border;
                var tap = border.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
                p = tap?.CommandParameter as Payment;
            }
            if (p == null && sender is Border border2)
            {
                var tap2 = border2.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
                p = tap2?.CommandParameter as Payment ?? border2.BindingContext as Payment;
            }

            if (p == null)
            {
                if ((sender as Border)?.BindingContext is Payment payment)
                    p = payment;
            }

            if (p != null)
            {
                _editingPayment = p;
                PartyNameEntry.Text = p.PartyName;
                AmountEntry.Text = p.Amount.ToString();
                BankNameEntry.Text = p.BankName;
                AccountNoEntry.Text = p.AccountNo;
                TransactionIdEntry.Text = p.TransactionId;
                ChequeNoEntry.Text = p.ChequeNo;
                IFSCEntry.Text = p.IFSC;
                RemarkEntry.Text = p.Remark;
                // ✅ FINAL SAFE FIX - આ એક જ Line વાપરો
                PaymentDatePicker.Date = p.PaymentDate is DateTime d2 ? d2 : DateTime.Now;

                for (int i = 0; i < PaymentTypePicker.Items.Count; i++)
                {
                    if (PaymentTypePicker.Items[i].Contains(p.PaymentType))
                    {
                        PaymentTypePicker.SelectedIndex = i;
                        break;
                    }
                }

                SaveButtonLabel.Text = $"✏ Update {p.PartyName}";
                OpenForm();
                OnPaymentTypeChanged(null, null);
            }
        }

        private async void OnDeletePaymentClicked(object sender, EventArgs e)
        {
            Payment? p = null;
            if (sender is Border border)
            {
                var tap = border.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
                p = tap?.CommandParameter as Payment ?? border.BindingContext as Payment;
            }

            if (p != null)
            {
                bool confirm = await DisplayAlert("🗑 Delete Payment?", $"👥 {p.PartyName} - 💰 Rs {p.Amount:N0}\n💳 {p.PaymentType} - 🏦 {p.BankName}\nDelete કરવું છે?", "🗑 Yes Delete", "❌ Cancel");
                if (confirm)
                {
                    await _service.DeletePayment(p.Id);
                    await LoadPayments();
                }
            }
        }

        private async void OnPrintPaymentClicked(object sender, EventArgs e)
        {
            try
            {
                Payment? p = null;
                if (sender is Border border)
                {
                    var tap = border.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
                    p = tap?.CommandParameter as Payment ?? border.BindingContext as Payment;
                }

                if (p != null)
                {
                    var path = await PrintService.PrintPaymentReceipt(p);
                    await DisplayAlert("🖨 Payment Receipt", $"👥 {p.PartyName} - Rs {p.Amount:N0}\n💳 {p.PaymentType} - 🏦 {p.BankName}\n🔢 UTR: {p.TransactionId}\n📄 PDF: {path}", "Open");
                    if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
                }
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

