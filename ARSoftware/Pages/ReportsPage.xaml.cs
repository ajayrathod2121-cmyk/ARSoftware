using ARSoftware.Services;
using ARSoftware.Models;

namespace ARSoftware.Pages
{
    public partial class ReportsPage : ContentPage
    {
        private SupabaseService _service;
        private List<JobworkEntry> _entries = new();
        private List<Payment> _payments = new();
        private List<Expense> _expenses = new();

        public ReportsPage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAllReports();
        }

        private async Task LoadAllReports()
        {
            try
            {
                _entries = await _service.GetEntries();
                _payments = await _service.GetPayments();
                _expenses = await _service.GetExpenses();

                // TOTAL - આટલી આવી, ગઈ, બાકી
                var totalBags = _entries.Sum(e => e.Bags);
                var deliveredBags = _entries.Sum(e => e.DeliveredBags);
                var pendingBags = _entries.Sum(e => e.PendingBags);
                var totalBilling = _entries.Sum(e => e.TotalAmount);

                var pendingAmt = 0m;
                foreach (var e in _entries.Where(x => x.PendingBags > 0))
                {
                    decimal perBagNet = e.Bags > 0 ? e.NetWeight / e.Bags : e.BagWeight;
                    if (perBagNet == 0) perBagNet = e.BagWeight;
                    pendingAmt += perBagNet * e.PendingBags * e.RatePerKg;
                }

                if (StockSummaryLabel != null)
                    StockSummaryLabel.Text = $"Total: {totalBags} | Delivered: {deliveredBags} | Pending: {pendingBags} Bags - Rs {pendingAmt:N0}";

                try
                {
                    var aavak = this.FindByName<Label>("TotalAavakLabel");
                    if (aavak != null) aavak.Text = $"{totalBags} Bags";
                    var del = this.FindByName<Label>("TotalDeliveredLabel");
                    if (del != null) del.Text = $"{deliveredBags} Bags";
                    var pend = this.FindByName<Label>("TotalPendingLabel");
                    if (pend != null) pend.Text = $"{pendingBags} Bags";
                }
                catch { }

                if (PaymentSummaryLabel != null)
                    PaymentSummaryLabel.Text = $"Rs {_payments.Sum(p => p.Amount):N0} - {_payments.Count} Payments";
                if (ExpenseSummaryLabel != null)
                    ExpenseSummaryLabel.Text = $"Rs {_expenses.Sum(e => e.Amount):N0} - {_expenses.Count} Expenses";

                // NEW: PARTY WISE STOCK - પાર્ટી વાઇસ આટલી આવી, ગઈ, બાકી
                var partyWise = _entries
                    .GroupBy(e => e.PartyName)
                    .Select(g =>
                    {
                        var total = g.Sum(x => x.Bags);
                        var delivered = g.Sum(x => x.DeliveredBags);
                        var pending = g.Sum(x => x.PendingBags);
                        var totalWt = g.Sum(x => x.TotalWeight);
                        var pendingAmtParty = g.Where(x => x.PendingBags > 0).Sum(x =>
                        {
                            decimal perBagNet = x.Bags > 0 ? x.NetWeight / x.Bags : x.BagWeight;
                            if (perBagNet == 0) perBagNet = x.BagWeight;
                            return perBagNet * x.PendingBags * x.RatePerKg;
                        });
                        var lastBill = g.OrderByDescending(x => x.EntryDate).FirstOrDefault();
                        return new
                        {
                            PartyName = g.Key,
                            TotalBags = total,
                            DeliveredBags = delivered,
                            PendingBags = pending,
                            TotalWeight = totalWt,
                            DeliveredWeight = totalWt * (total > 0 ? (decimal)delivered / total : 0),
                            PendingWeight = totalWt * (total > 0 ? (decimal)pending / total : 0),
                            PendingAmount = pendingAmtParty,
                            BillsCount = g.Count(),
                            LastBillInfo = lastBill != null ? $"Last: {lastBill.BillNo} - {lastBill.EntryDate:dd/MM/yyyy}" : "",
                            // જૂના XAML માટે
                            Bags = total,
                            Amount = g.Sum(x => x.TotalAmount),
                            BillNo = $"{g.Count()} Bills",
                            Date = lastBill?.EntryDate.ToString("dd/MM/yyyy") ?? "",
                            ItemName = $"{total} Total",
                            NetWeight = totalWt,
                            VehicleNo = lastBill?.VehicleNo ?? "",
                            DisplayBags = $"{total} | Del {delivered} | Pend {pending}",
                            Status = pending == 0 ? "Delivered" : delivered == 0 ? "Pending" : "Partial",
                            StatusColor = pending == 0 ? "#10B981" : delivered == 0 ? "#FF3B30" : "#FF6B00"
                        };
                    })
                    .OrderByDescending(x => x.PendingBags)
                    .ToList();

                // Party Wise Collection હોય તો Set કરો - ન હોય તો Error નહીં
                try
                {
                    var partyCollection = this.FindByName<CollectionView>("PartyWiseStockCollection");
                    if (partyCollection != null)
                        partyCollection.ItemsSource = partyWise;
                }
                catch { }

                // BILL WISE DETAIL - પણ બતાવો
                var stockDetails = _entries.OrderByDescending(e => e.EntryDate).Select(e => new
                {
                    BillNo = e.BillNo,
                    Date = e.EntryDate.ToString("dd/MM/yyyy"),
                    PartyName = e.PartyName,
                    ItemName = e.ItemName,
                    TotalBags = e.Bags,
                    DeliveredBags = e.DeliveredBags,
                    PendingBags = e.PendingBags,
                    DisplayBags = $"{e.Bags} Bags | Delivered {e.DeliveredBags} | Pending {e.PendingBags}",
                    NetWeight = e.NetWeight,
                    VehicleNo = e.VehicleNo,
                    Amount = e.TotalAmount,
                    PendingAmount = e.PendingBags > 0 ? (e.Bags > 0 ? (e.NetWeight / e.Bags * e.PendingBags * e.RatePerKg) : 0) : 0,
                    Status = e.Status,
                    StatusColor = e.Status == "Delivered" ? "#10B981" : e.Status == "Partial" ? "#FF6B00" : "#FF3B30",
                    Bags = e.Bags
                }).ToList();

                if (StockDetailsCollection != null)
                {
                    // જો Party Wise હોય તો Bill Wise ને Party Wise માં નાખો - નહીંતર Stock માં Bill Wise બતાવો
                    var hasPartyWise = this.FindByName<CollectionView>("PartyWiseStockCollection") != null;
                    if (!hasPartyWise)
                    {
                        // Party Wise XAML નથી - તો Stock માં જ Party Wise બતાવો
                        StockDetailsCollection.ItemsSource = partyWise;
                    }
                    else
                    {
                        StockDetailsCollection.ItemsSource = stockDetails;
                    }
                }

                var paymentDetails = _payments.OrderByDescending(p => p.PaymentDate).Select(p => new
                {
                    Date = p.PaymentDate.ToString("dd/MM/yyyy"),
                    PartyName = p.PartyName,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType,
                    BankName = p.BankName,
                    TransactionId = p.TransactionId,
                    ChequeNo = p.ChequeNo,
                    DisplayBank = $"{p.BankName} {p.TransactionId} {p.ChequeNo}".Trim(),
                    Remark = p.Remark
                }).ToList();

                if (PaymentDetailsCollection != null)
                    PaymentDetailsCollection.ItemsSource = paymentDetails;

                var expenseDetails = _expenses.OrderByDescending(e => e.ExpenseDate).Select(e => new
                {
                    Date = e.ExpenseDate.ToString("dd/MM/yyyy"),
                    Title = e.Title,
                    Category = e.Category,
                    Amount = e.Amount,
                    Remark = e.Remark
                }).ToList();

                if (ExpenseDetailsCollection != null)
                    ExpenseDetailsCollection.ItemsSource = expenseDetails;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnStockReportDetailed(object sender, EventArgs e)
        {
            var totalBags = _entries.Sum(x => x.Bags);
            var deliveredBags = _entries.Sum(x => x.DeliveredBags);
            var pendingBags = _entries.Sum(x => x.PendingBags);

            var partyWiseText = _entries.GroupBy(x => x.PartyName)
                .Select(g => $"{g.Key}: {g.Sum(x => x.Bags)} આવી | {g.Sum(x => x.DeliveredBags)} ગઈ | {g.Sum(x => x.PendingBags)} બાકી")
                .ToList();

            await DisplayAlert("Party Wise Stock - પાર્ટી વાઇસ",
                $"કુલ: {totalBags} | ડિલિવર: {deliveredBags} | બાકી: {pendingBags}\n\n" + string.Join("\n", partyWiseText.Take(20)), "OK");
        }

        private async void OnPaymentReportDetailed(object sender, EventArgs e)
        {
            var total = _payments.Sum(p => p.Amount);
            var cash = _payments.Where(p => p.PaymentType == "Cash").Sum(p => p.Amount);
            var online = _payments.Where(p => p.PaymentType != "Cash").Sum(p => p.Amount);
            await DisplayAlert("Payment Report", $"Total: Rs {total:N0}\nCash: Rs {cash:N0}\nBank: Rs {online:N0}\nEntries: {_payments.Count}", "OK");
        }

        private async void OnExpenseReportDetailed(object sender, EventArgs e)
        {
            var total = _expenses.Sum(x => x.Amount);
            await DisplayAlert("Expense Report", $"Total: Rs {total:N0}\nEntries: {_expenses.Count}", "OK");
        }

        private async void OnAllReport(object sender, EventArgs e)
        {
            var totalBags = _entries.Sum(x => x.Bags);
            var deliveredBags = _entries.Sum(x => x.DeliveredBags);
            var pendingBags = _entries.Sum(x => x.PendingBags);
            var totalBilling = _entries.Sum(x => x.TotalAmount);
            var totalPay = _payments.Sum(p => p.Amount);
            var baki = totalBilling - totalPay;

            var partyWise = _entries.GroupBy(x => x.PartyName)
                .Select(g => $"\n{g.Key}: {g.Sum(x => x.Bags)}/{g.Sum(x => x.DeliveredBags)}/{g.Sum(x => x.PendingBags)}")
                .Take(15);

            await DisplayAlert("Full Report - Party Wise",
                $"આવક: {totalBags} | ડિલિવર: {deliveredBags} | બાકી: {pendingBags}\nBilling: Rs {totalBilling:N0}\nBaki: Rs {baki:N0}\n\nપાર્ટી વાઇસ:{string.Join("", partyWise)}", "OK");
        }

        private async void OnPrintStockReport(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintDetailedStockReport(_entries);
                await DisplayAlert("Stock PDF - Party Wise", $"Total: {_entries.Sum(x => x.Bags)} | Delivered: {_entries.Sum(x => x.DeliveredBags)} | Pending: {_entries.Sum(x => x.PendingBags)}\n{path}", "Open");
                if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnPrintPaymentReport(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintDetailedPaymentReport(_payments);
                await DisplayAlert("Payment PDF", path, "Open");
                if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnPrintExpenseReport(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintDetailedExpenseReport(_expenses);
                await DisplayAlert("Expense PDF", path, "Open");
                if (File.Exists(path)) await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnPrintFullReport(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintFullReport(_entries, _payments, _expenses);
                await DisplayAlert("Full PDF - Party Wise", $"Pending: {_entries.Sum(x => x.PendingBags)} Bags\n{path}", "Open");
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

