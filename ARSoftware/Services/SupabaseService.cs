using ARSoftware.Models;

namespace ARSoftware.Services
{
    public class SupabaseService
    {
        private List<Party> _parties = new List<Party> { new Party { Id = 1, Name = "Rameshbhai", Mobile = "9876543210", City = "Rajkot" }, new Party { Id = 2, Name = "Suresh Trading", Mobile = "9876543211", City = "Gondal" }, new Party { Id = 3, Name = "Mahavir Masala", Mobile = "9876543212", City = "Jetpur" } };
        private List<Item> _items = new List<Item> { new Item { Id = 1, Name = "Mirchi", RatePerKg = 50, Category = "Masala" }, new Item { Id = 2, Name = "Dhana", RatePerKg = 40, Category = "Masala" }, new Item { Id = 3, Name = "Jeera", RatePerKg = 80, Category = "Masala" } };
        private List<JobworkEntry> _entries = new List<JobworkEntry> { new JobworkEntry { Id = 1, BillNo = "B001", PartyId = 1, PartyName = "Rameshbhai", ItemName = "Mirchi", Bags = 1000, BagWeight = 40, TotalWeight = 40000, NetWeight = 39500, Kharabo = 500, RatePerKg = 50, TotalAmount = 1975000, DeliveredBags = 1000, PendingBags = 0, VehicleNo = "GJ03 AB 1234", Status = "Delivered", EntryDate = DateTime.Now }, new JobworkEntry { Id = 2, BillNo = "B002", PartyId = 2, PartyName = "Suresh Trading", ItemName = "Dhana", Bags = 15, BagWeight = 40, TotalWeight = 600, NetWeight = 595, TotalAmount = 23800, VehicleNo = "GJ05 XY 9988", Status = "Delivered", DeliveredBags = 15, PendingBags = 0, EntryDate = DateTime.Now.AddHours(-2) } };
        private List<Payment> _payments = new List<Payment> { new Payment { Id = 1, PartyId = 1, PartyName = "Rameshbhai", Amount = 20000, PaymentType = "Cash", PaymentDate = DateTime.Now.AddDays(-1) } };
        private List<Expense> _expenses = new List<Expense> { new Expense { Id = 1, Title = "Diesel", Category = "Diesel", Amount = 2000, ExpenseDate = DateTime.Now } };

        public Task<List<Party>> GetParties() => Task.FromResult(_parties.OrderBy(x => x.Name).ToList());
        public Task AddParty(Party p) { p.Id = _parties.Count > 0 ? _parties.Max(x => x.Id) + 1 : 1; p.CreatedAt = DateTime.Now; _parties.Add(p); return Task.CompletedTask; }
        public Task UpdateParty(Party p) { var ex = _parties.FirstOrDefault(x => x.Id == p.Id); if (ex != null) { ex.Name = p.Name; ex.Mobile = p.Mobile; ex.City = p.City; ex.Address = p.Address; } return Task.CompletedTask; }
        public Task DeleteParty(long id) { _parties.RemoveAll(x => x.Id == id); return Task.CompletedTask; }

        public Task<List<Item>> GetItems() => Task.FromResult(_items.ToList());
        public Task AddItem(Item i) { i.Id = _items.Count > 0 ? _items.Max(x => x.Id) + 1 : 1; i.CreatedAt = DateTime.Now; _items.Add(i); return Task.CompletedTask; }
        public Task UpdateItem(Item i) { var ex = _items.FirstOrDefault(x => x.Id == i.Id); if (ex != null) { ex.Name = i.Name; ex.RatePerKg = i.RatePerKg; ex.Category = i.Category; } return Task.CompletedTask; }
        public Task DeleteItem(long id) { _items.RemoveAll(x => x.Id == id); return Task.CompletedTask; }

        public Task<List<JobworkEntry>> GetEntries() => Task.FromResult(_entries.OrderByDescending(x => x.Id).ToList());

        public Task AddEntry(JobworkEntry e)
        {
            if (!string.IsNullOrWhiteSpace(e.PartyName))
            {
                var existingParty = _parties.FirstOrDefault(p => p.Name.Trim().ToLower() == e.PartyName.Trim().ToLower());
                if (existingParty == null)
                {
                    var newParty = new Party { Id = _parties.Count > 0 ? _parties.Max(x => x.Id) + 1 : 1, Name = e.PartyName.Trim(), City = "Rajkot", Mobile = "", CreatedAt = DateTime.Now };
                    _parties.Add(newParty);
                    e.PartyId = newParty.Id;
                }
                else e.PartyId = existingParty.Id;
            }

            if (e.Bags > 0)
            {
                if (e.DeliveredBags == 0 && e.PendingBags == 0)
                {
                    e.DeliveredBags = e.Bags;
                    e.PendingBags = 0;
                    e.Status = "Delivered";
                }

                if (e.TotalAmount == 0)
                {
                    decimal net = e.NetWeight;
                    if (net == 0) net = (e.Bags * e.BagWeight) - e.Kharabo;
                    if (net == 0) net = e.Bags * e.BagWeight;
                    if (e.DeliveredBags > 0 && e.DeliveredBags < e.Bags)
                    {
                        decimal perBagNet = e.Bags > 0 ? net / e.Bags : e.BagWeight;
                        net = perBagNet * e.DeliveredBags;
                    }
                    e.TotalAmount = net * e.RatePerKg;
                }

                e.PendingBags = e.Bags - e.DeliveredBags;
                if (e.PendingBags > 0 && e.DeliveredBags > 0) e.Status = "Partial";
                else if (e.PendingBags == 0) e.Status = "Delivered";
                else e.Status = "Pending";
            }

            e.Id = _entries.Count > 0 ? _entries.Max(x => x.Id) + 1 : 1;
            e.CreatedAt = DateTime.Now;
            _entries.Add(e);
            return Task.CompletedTask;
        }

        public Task UpdateEntry(JobworkEntry e)
        {
            var ex = _entries.FirstOrDefault(x => x.Id == e.Id);
            if (ex != null)
            {
                if (!string.IsNullOrWhiteSpace(e.PartyName))
                {
                    var existingParty = _parties.FirstOrDefault(p => p.Name.Trim().ToLower() == e.PartyName.Trim().ToLower());
                    if (existingParty == null)
                    {
                        var newParty = new Party { Id = _parties.Count > 0 ? _parties.Max(x => x.Id) + 1 : 1, Name = e.PartyName.Trim(), City = "Rajkot", CreatedAt = DateTime.Now };
                        _parties.Add(newParty);
                        e.PartyId = newParty.Id;
                    }
                    else e.PartyId = existingParty.Id;
                }

                if (e.Bags > 0)
                {
                    if (e.TotalAmount == 0)
                    {
                        decimal net = e.NetWeight;
                        if (net == 0) net = (e.Bags * e.BagWeight) - e.Kharabo;
                        if (net == 0) net = e.Bags * e.BagWeight;
                        if (e.DeliveredBags > 0 && e.DeliveredBags < e.Bags)
                        {
                            decimal perBagNet = e.Bags > 0 ? net / e.Bags : e.BagWeight;
                            net = perBagNet * e.DeliveredBags;
                        }
                        e.TotalAmount = net * e.RatePerKg;
                    }
                    e.PendingBags = e.Bags - e.DeliveredBags;
                    if (e.PendingBags > 0 && e.DeliveredBags > 0) e.Status = "Partial";
                    else if (e.PendingBags == 0) e.Status = "Delivered";
                    else e.Status = "Pending";
                }

                ex.BillNo = e.BillNo; ex.PartyId = e.PartyId; ex.PartyName = e.PartyName; ex.ItemId = e.ItemId; ex.ItemName = e.ItemName; ex.Bags = e.Bags; ex.BagWeight = e.BagWeight; ex.TotalWeight = e.TotalWeight; ex.BridgeWeight = e.BridgeWeight; ex.Kharabo = e.Kharabo; ex.NetWeight = e.NetWeight; ex.VehicleNo = e.VehicleNo; ex.RatePerKg = e.RatePerKg; ex.TotalAmount = e.TotalAmount; ex.Status = e.Status; ex.DeliveredBags = e.DeliveredBags; ex.PendingBags = e.PendingBags; ex.EntryDate = e.EntryDate;
            }
            return Task.CompletedTask;
        }

        public Task DeliverPartial(long entryId, int deliverBags)
        {
            var ex = _entries.FirstOrDefault(x => x.Id == entryId);
            if (ex != null && deliverBags > 0 && deliverBags <= ex.PendingBags)
            {
                ex.DeliveredBags += deliverBags;
                ex.PendingBags = ex.Bags - ex.DeliveredBags;
                decimal perBagNet = ex.Bags > 0 ? ex.NetWeight / ex.Bags : ex.BagWeight;
                if (perBagNet == 0) perBagNet = ex.BagWeight;
                decimal deliveredNet = perBagNet * ex.DeliveredBags;
                decimal perBagKharabo = ex.Bags > 0 ? ex.Kharabo / ex.Bags : 0;
                deliveredNet -= perBagKharabo * ex.DeliveredBags;
                if (deliveredNet == 0) deliveredNet = ex.DeliveredBags * ex.BagWeight;
                ex.TotalAmount = deliveredNet * ex.RatePerKg;
                ex.Status = ex.PendingBags > 0 ? "Partial" : "Delivered";
            }
            return Task.CompletedTask;
        }

        public Task DeleteEntry(long id) { _entries.RemoveAll(x => x.Id == id); return Task.CompletedTask; }

        public Task<List<Payment>> GetPayments() => Task.FromResult(_payments.OrderByDescending(x => x.Id).ToList());
        public Task<List<Payment>> GetPaymentsByParty(long partyId)
        {
            var party = _parties.FirstOrDefault(p => p.Id == partyId);
            if (party != null)
                return Task.FromResult(_payments.Where(x => x.PartyId == partyId || x.PartyName.ToLower() == party.Name.ToLower()).OrderByDescending(x => x.Id).ToList());
            return Task.FromResult(_payments.Where(x => x.PartyId == partyId).OrderByDescending(x => x.Id).ToList());
        }
        public Task AddPayment(Payment p) { p.Id = _payments.Count > 0 ? _payments.Max(x => x.Id) + 1 : 1; p.CreatedAt = DateTime.Now; _payments.Add(p); return Task.CompletedTask; }

        // FIXED: Bank Detail સાથે Update - Premium Dashboard માટે
        public Task UpdatePayment(Payment p)
        {
            var ex = _payments.FirstOrDefault(x => x.Id == p.Id);
            if (ex != null)
            {
                ex.PartyId = p.PartyId;
                ex.PartyName = p.PartyName;
                ex.Amount = p.Amount;
                ex.PaymentType = p.PaymentType;
                ex.PaymentDate = p.PaymentDate;
                ex.Remark = p.Remark;
                ex.BankName = p.BankName;
                ex.AccountNo = p.AccountNo;
                ex.IFSC = p.IFSC;
                ex.TransactionId = p.TransactionId;
                ex.ChequeNo = p.ChequeNo;
            }
            return Task.CompletedTask;
        }
        public Task DeletePayment(long id) { _payments.RemoveAll(x => x.Id == id); return Task.CompletedTask; }

        public Task<decimal> GetPartyBaki(long partyId)
        {
            var party = _parties.FirstOrDefault(p => p.Id == partyId);
            decimal totalBill = 0; decimal totalPaid = 0;
            if (party != null)
            {
                totalBill = _entries.Where(x => x.PartyId == partyId || x.PartyName.Trim().ToLower() == party.Name.Trim().ToLower()).Sum(x => x.TotalAmount);
                totalPaid = _payments.Where(x => x.PartyId == partyId || x.PartyName.Trim().ToLower() == party.Name.Trim().ToLower()).Sum(x => x.Amount);
            }
            else
            {
                totalBill = _entries.Where(x => x.PartyId == partyId).Sum(x => x.TotalAmount);
                totalPaid = _payments.Where(x => x.PartyId == partyId).Sum(x => x.Amount);
            }
            return Task.FromResult(totalBill - totalPaid);
        }
        public Task<decimal> GetTotalBaki() { var totalBill = _entries.Sum(x => x.TotalAmount); var totalPaid = _payments.Sum(x => x.Amount); return Task.FromResult(totalBill - totalPaid); }

        // FIXED: GetBills - Dashboard માં Total Billing માટે - આ Missing હતું!
        public Task<List<Bill>> GetBills()
        {
            var bills = _entries.Select(e => new Bill
            {
                Id = e.Id,
                PartyId = e.PartyId,
                PartyName = e.PartyName,
                BillNo = e.BillNo,
                Amount = e.TotalAmount,
                TotalAmount = e.TotalAmount,
                BillDate = e.EntryDate,
                CreatedAt = e.CreatedAt,
                Status = e.Status
            }).OrderByDescending(x => x.Id).ToList();
            return Task.FromResult(bills);
        }

        public Task<List<Expense>> GetExpenses() => Task.FromResult(_expenses.OrderByDescending(x => x.Id).ToList());
        public Task AddExpense(Expense e) { e.Id = _expenses.Count > 0 ? _expenses.Max(x => x.Id) + 1 : 1; e.CreatedAt = DateTime.Now; _expenses.Add(e); return Task.CompletedTask; }
        public Task UpdateExpense(Expense e) { var ex = _expenses.FirstOrDefault(x => x.Id == e.Id); if (ex != null) { ex.Title = e.Title; ex.Category = e.Category; ex.Amount = e.Amount; ex.ExpenseDate = e.ExpenseDate; ex.Remark = e.Remark; } return Task.CompletedTask; }
        public Task DeleteExpense(long id) { _expenses.RemoveAll(x => x.Id == id); return Task.CompletedTask; }

        public bool IsConnected => false;
        public void SetCredentials(string url, string key) { }
    }

    // FIXED: Bill Model - Dashboard માં Billing માટે જરૂરી
    public class Bill
    {
        public long Id { get; set; }
        public long PartyId { get; set; }
        public string PartyName { get; set; } = "";
        public string BillNo { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BillDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
    }
}