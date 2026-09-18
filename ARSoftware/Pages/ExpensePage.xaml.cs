using ARSoftware.Models;
using ARSoftware.Services;

namespace ARSoftware.Pages
{
    public partial class ExpensePage : ContentPage
    {
        private SupabaseService _service;
        private List<Expense> _expenses = new();

        public ExpensePage(SupabaseService service)
        {
            InitializeComponent();
            _service = service;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadExpenses();
        }

        private async Task LoadExpenses()
        {
            _expenses = await _service.GetExpenses();
            ExpenseCollection.ItemsSource = _expenses.OrderByDescending(e => e.ExpenseDate).ToList();
            TotalLabel.Text = $"Rs {_expenses.Sum(e => e.Amount):N0} - {_expenses.Count} Entries - Today Rs {_expenses.Where(e => e.ExpenseDate.Date == DateTime.Now.Date).Sum(e => e.Amount):N0}";
        }

        private Expense GetExpenseFromSender(object sender)
        {
            // ✅ FIXED: Border હોય કે Button - બંનેમાંથી Expense મળશે
            try
            {
                var bo = sender as BindableObject;
                var ctx = bo?.BindingContext;
                if (ctx is Expense direct) return direct;

                // જો CommandParameter વાળું હોય તો
                if (bo is Border border)
                {
                    var gesture = border.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
                    if (gesture?.CommandParameter is Expense paramExp) return paramExp;
                }
                return ctx as Expense;
            }
            catch { return null; }
        }

        private async void OnAddExpenseClicked(object sender, EventArgs e)
        {
            try
            {
                string title = await DisplayPromptAsync("Add Expense", "Title (દા.ત. Diesel, Labour):", "Next", "Cancel", "", 50);
                if (string.IsNullOrWhiteSpace(title)) return;

                string category = await DisplayActionSheet("Category", "Cancel", null, "Diesel", "Labour", "Rent", "Electric", "Tea", "Other");
                if (category == null || category == "Cancel") category = "Other";

                string amt = await DisplayPromptAsync("Amount", $"{title} - Rs:", "Next", "Cancel", "", 10, Keyboard.Numeric);
                if (string.IsNullOrWhiteSpace(amt) || !decimal.TryParse(amt, out decimal amount)) return;

                string remark = await DisplayPromptAsync("Remark", "Remark (Optional):", "Save", "Cancel", "", 100);

                var expense = new Expense
                {
                    Title = title.Trim(),
                    Category = category,
                    Amount = amount,
                    ExpenseDate = DateTime.Now,
                    Remark = remark ?? "",
                    CreatedAt = DateTime.Now
                };

                await _service.AddExpense(expense);
                await LoadExpenses();
                await DisplayAlert("✅ Saved", $"{title} - Rs {amount:N0} - {category}", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        private async void OnEditExpenseClicked(object sender, TappedEventArgs e)
        {
            var ex = GetExpenseFromSender(sender) ?? e.Parameter as Expense;
            if (ex == null) return;

            try
            {
                string t = await DisplayPromptAsync("Edit Title", "Title:", "Next", "Cancel", ex.Title, 50);
                if (string.IsNullOrWhiteSpace(t)) return;

                string category = await DisplayActionSheet("Category", "Cancel", null, "Diesel", "Labour", "Rent", "Electric", "Tea", "Other");
                if (category == null || category == "Cancel") category = ex.Category;

                string amt = await DisplayPromptAsync("Edit Amount", "Rs:", "Next", "Cancel", ex.Amount.ToString(), 10, Keyboard.Numeric);
                if (!decimal.TryParse(amt, out decimal amount)) return;

                string remark = await DisplayPromptAsync("Edit Remark", "Remark:", "Save", "Cancel", ex.Remark, 100);

                ex.Title = t.Trim();
                ex.Category = category;
                ex.Amount = amount;
                ex.Remark = remark ?? "";

                await _service.UpdateExpense(ex);
                await LoadExpenses();
            }
            catch (Exception ex2) { await DisplayAlert("Error", ex2.Message, "OK"); }
        }

        private async void OnDeleteExpenseClicked(object sender, TappedEventArgs e)
        {
            var ex = GetExpenseFromSender(sender) ?? e.Parameter as Expense;
            if (ex == null) return;

            if (await DisplayAlert("Delete?", $"{ex.Title} - Rs {ex.Amount:N0}\nDelete?", "Yes", "Cancel"))
            {
                await _service.DeleteExpense(ex.Id);
                await LoadExpenses();
            }
        }

        private async void OnPrintExpenseClicked(object sender, EventArgs e)
        {
            try
            {
                var path = await PrintService.PrintDetailedExpenseReport(_expenses);
                await DisplayAlert("Expense PDF", $"PDF Generated!\nTotal: Rs {_expenses.Sum(x => x.Amount):N0}\n{path}", "Open");
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

