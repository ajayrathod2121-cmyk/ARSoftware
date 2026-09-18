using System.Text;
using ARSoftware.Models;

namespace ARSoftware.Services
{
    public class PrintService
    {
        private static int ToInt(object v) { try { return Convert.ToInt32(v); } catch { return 0; } }
        private static decimal ToDec(object v) { try { return Convert.ToDecimal(v); } catch { return 0m; } }
        private static string ToStr(object v) => v?.ToString() ?? "";
        private static DateTime ToDate(object v) { try { return Convert.ToDateTime(v); } catch { return DateTime.Now; } }

        // Business Details માંથી Header બનાવો
        private static BusinessProfile Biz => BusinessService.GetBusiness();
        private static string HeaderName => string.IsNullOrWhiteSpace(Biz.BusinessName) ? "" : Biz.BusinessName;
        private static string HeaderLine => string.IsNullOrWhiteSpace(Biz.BusinessName) ? "" : $"{Biz.BusinessName}{(string.IsNullOrWhiteSpace(Biz.City) ? "" : $" - {Biz.City}")}";
        private static string FooterLine
        {
            get
            {
                var f = "";
                if (!string.IsNullOrWhiteSpace(Biz.Address)) f += $"{Biz.Address} ";
                if (!string.IsNullOrWhiteSpace(Biz.City)) f += $"{Biz.City} ";
                if (!string.IsNullOrWhiteSpace(Biz.Mobile)) f += $"| Mo: {Biz.Mobile} ";
                if (!string.IsNullOrWhiteSpace(Biz.GST)) f += $"| GST: {Biz.GST}";
                return f.Trim(' ', '|');
            }
        }

        public static async Task<string> PrintBillPDF(JobworkEntry entry) => await GenerateBillHtml(entry);

        public static async Task<string> PrintBill(string billNo, string partyName, string itemName, object bags, object netWeight, object totalAmount, object vehicleNo, object status)
        {
            var e = new JobworkEntry { BillNo = ToStr(billNo), PartyName = ToStr(partyName), ItemName = ToStr(itemName), Bags = ToInt(bags), NetWeight = ToDec(netWeight), TotalAmount = ToDec(totalAmount), VehicleNo = ToStr(vehicleNo), Status = ToStr(status), EntryDate = DateTime.Now };
            return await GenerateBillHtml(e);
        }

        public static async Task<string> PrintBillPDF(string billNo, string partyName, string itemName, object bags, object bagWeight, object totalWeight, object bridgeWeight, object kharabo, object netWeight, object vehicleNo, object rate, object totalAmount, object pendingBags, object deliveredBags, object status)
        {
            var e = new JobworkEntry { BillNo = ToStr(billNo), PartyName = ToStr(partyName), ItemName = ToStr(itemName), Bags = ToInt(bags), BagWeight = ToDec(bagWeight), TotalWeight = ToDec(totalWeight), BridgeWeight = ToDec(bridgeWeight), Kharabo = ToDec(kharabo), NetWeight = ToDec(netWeight), VehicleNo = ToStr(vehicleNo), RatePerKg = ToDec(rate), TotalAmount = ToDec(totalAmount), PendingBags = ToInt(pendingBags), DeliveredBags = ToInt(deliveredBags), Status = ToStr(status), EntryDate = DateTime.Now };
            return await GenerateBillHtml(e);
        }

        public static async Task<string> PrintBillPDF(string billNo, string partyName, string itemName, object bags, object bagWeight, object totalWeight, object bridgeWeight, object kharabo, object netWeight, object vehicleNo, object rate, object totalAmount, object pendingBags, object deliveredBags, object status, object entryDate)
        {
            var e = new JobworkEntry { BillNo = ToStr(billNo), PartyName = ToStr(partyName), ItemName = ToStr(itemName), Bags = ToInt(bags), BagWeight = ToDec(bagWeight), TotalWeight = ToDec(totalWeight), BridgeWeight = ToDec(bridgeWeight), Kharabo = ToDec(kharabo), NetWeight = ToDec(netWeight), VehicleNo = ToStr(vehicleNo), RatePerKg = ToDec(rate), TotalAmount = ToDec(totalAmount), PendingBags = ToInt(pendingBags), DeliveredBags = ToInt(deliveredBags), Status = ToStr(status), EntryDate = ToDate(entryDate) };
            return await GenerateBillHtml(e);
        }

        // ✅ PAYMENT RECEIPT - Business Name સાથે
        public static async Task<string> PrintPaymentReceipt(Payment p)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} .bill{border:3px solid #10B981;border-radius:15px;padding:25px;max-width:800px;margin:auto} .header{text-align:center;border-bottom:2px solid #10B981;padding-bottom:15px} h2{color:#10B981} table{width:100%;border-collapse:collapse;margin-top:15px} th,td{border:1px solid #ddd;padding:10px;text-align:left} th{background:#f0fff4;width:30%} .total{background:#10B981;color:white;padding:15px;border-radius:10px;text-align:center;font-size:22px;margin-top:20px}</style></head><body>");
            sb.Append("<div class='bill'>");
            // DYNAMIC HEADER
            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
                sb.Append($"<div class='header'><h2>{biz.BusinessName.ToUpper()} - PAYMENT RECEIPT</h2><p>{FooterLine}</p></div>");
            else
                sb.Append("<div class='header'><h2>PAYMENT RECEIPT</h2></div>");

            sb.Append($"<div style='text-align:center;margin:15px 0'><h3 style='background:#000;color:white;padding:10px;border-radius:8px;display:inline-block'>RECEIPT - {p.PaymentDate:dd/MM/yyyy}</h3></div>");
            sb.Append("<table>");
            sb.Append($"<tr><th>Party Name</th><td><b>{p.PartyName}</b></td></tr>");
            sb.Append($"<tr><th>Amount</th><td><b style='color:#10B981;font-size:18px'>Rs {p.Amount:N0}/-</b></td></tr>");
            sb.Append($"<tr><th>Payment Type</th><td><b>{p.PaymentType}</b></td></tr>");
            sb.Append($"<tr><th>Bank Name</th><td>{p.BankName}</td></tr>");
            sb.Append($"<tr><th>Account No / UPI ID</th><td>{p.AccountNo}</td></tr>");
            sb.Append($"<tr><th>UTR / RTGS / NEFT No</th><td><b style='color:#FF6B00'>{p.TransactionId}</b></td></tr>");
            sb.Append($"<tr><th>Cheque No</th><td>{p.ChequeNo}</td></tr>");
            sb.Append($"<tr><th>IFSC Code</th><td>{p.IFSC}</td></tr>");
            sb.Append($"<tr><th>Payment Date</th><td>{p.PaymentDate:dd MMMM yyyy}</td></tr>");
            sb.Append($"<tr><th>Remark</th><td>{p.Remark}</td></tr>");
            sb.Append("</table>");
            sb.Append($"<div class='total'>PAID: Rs {p.Amount:N0}/- via {p.PaymentType}</div>");
            sb.Append($"<p style='text-align:center;margin-top:20px;font-size:12px;color:#777'>Thank You! - Generated by {HeaderName}</p>");
            sb.Append("</div></body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"PaymentReceipt_{p.PartyName}_{p.Amount}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }

        // ✅ STOCK REPORT - Business Name સાથે
        public static async Task<string> PrintDetailedStockReport(List<JobworkEntry> entries)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} table{width:100%;border-collapse:collapse} th,td{border:1px solid #000;padding:8px;text-align:left} th{background:#FF6B00;color:white} h2{color:#FF6B00}</style></head><body>");
            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
                sb.Append($"<h2>{HeaderLine} - Stock Report - {DateTime.Now:dd MMM yyyy}</h2><p>{FooterLine}</p>");
            else
                sb.Append($"<h2>Stock Report - {DateTime.Now:dd MMM yyyy}</h2>");

            var totalBags = entries.Sum(x => x.Bags);
            var deliveredBags = entries.Sum(x => x.DeliveredBags);
            var pendingBags = entries.Sum(x => x.PendingBags);
            var totalBilling = entries.Sum(x => x.TotalAmount);

            sb.Append($"<h3>Total Order: {totalBags} Bags | Delivered: {deliveredBags} Bags | Pending Stock: {pendingBags} Bags</h3>");
            sb.Append($"<h3>Total Billing: Rs {totalBilling:N0}</h3><hr/>");
            sb.Append("<table><tr><th>Bill No</th><th>Date</th><th>Party</th><th>Item</th><th>Total</th><th>Delivered</th><th>Pending</th><th>Net Wt</th><th>Vehicle</th><th>Amount</th><th>Status</th></tr>");
            foreach (var e in entries.OrderByDescending(x => x.EntryDate))
            {
                var rowColor = e.Status == "Partial" ? "background:#FFF7ED" : e.Status == "Pending" ? "background:#FFE5E5" : "background:#F0FFF4";
                sb.Append($"<tr style='{rowColor}'><td>{e.BillNo}</td><td>{e.EntryDate:dd/MM/yyyy}</td><td>{e.PartyName}</td><td>{e.ItemName}</td><td><b>{e.Bags}</b></td><td style='color:green'><b>{e.DeliveredBags}</b></td><td style='color:red'><b>{e.PendingBags}</b></td><td>{e.NetWeight:N0} KG</td><td>{e.VehicleNo}</td><td>Rs {e.TotalAmount:N0}</td><td>{e.Status}</td></tr>");
            }
            sb.Append("</table>");
            sb.Append($"<h3 style='margin-top:20px'>Summary: Total {totalBags} | Delivered {deliveredBags} | Pending {pendingBags} Bags</h3>");
            sb.Append("</body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"Stock_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }

        // ✅ PAYMENT REPORT - Business Name સાથે
        public static async Task<string> PrintDetailedPaymentReport(List<Payment> payments)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} table{width:100%;border-collapse:collapse} th,td{border:1px solid #000;padding:8px;text-align:left;font-size:11px} th{background:#0066FF;color:white} h2{color:#0066FF}</style></head><body>");
            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
                sb.Append($"<h2>{HeaderLine} - Payment Report - {DateTime.Now:dd MMM yyyy}</h2><p>{FooterLine}</p>");
            else
                sb.Append($"<h2>Payment Report - {DateTime.Now:dd MMM yyyy}</h2>");

            var total = payments.Sum(x => x.Amount);
            var cash = payments.Where(x => x.PaymentType == "Cash").Sum(x => x.Amount);
            var bank = payments.Where(x => x.PaymentType != "Cash").Sum(x => x.Amount);
            sb.Append($"<h3>Total: Rs {total:N0} | Cash: Rs {cash:N0} | Bank/Online: Rs {bank:N0} | Entries: {payments.Count}</h3><hr/>");
            sb.Append("<table><tr><th>Date</th><th>Party Name</th><th>Amount</th><th>Type</th><th>Bank Name</th><th>Account/UPI</th><th>UTR/RTGS/NEFT</th><th>Cheque No</th><th>IFSC</th><th>Remark</th></tr>");
            foreach (var p in payments.OrderByDescending(x => x.PaymentDate))
                sb.Append($"<tr><td>{p.PaymentDate:dd/MM/yyyy}</td><td>{p.PartyName}</td><td><b>Rs {p.Amount:N0}</b></td><td>{p.PaymentType}</td><td>{p.BankName}</td><td>{p.AccountNo}</td><td style='color:#FF6B00'><b>{p.TransactionId}</b></td><td>{p.ChequeNo}</td><td>{p.IFSC}</td><td>{p.Remark}</td></tr>");
            sb.Append("</table></body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"Payment_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }

        public static async Task<string> PrintDetailedExpenseReport(List<Expense> expenses)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} table{width:100%;border-collapse:collapse} th,td{border:1px solid #000;padding:8px;text-align:left} th{background:#FF0000;color:white} h2{color:#FF0000}</style></head><body>");
            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
                sb.Append($"<h2>{HeaderLine} - Expense Report - {DateTime.Now:dd MMM yyyy}</h2>");
            else
                sb.Append($"<h2>Expense Report - {DateTime.Now:dd MMM yyyy}</h2>");

            sb.Append($"<h3>Total Expense: Rs {expenses.Sum(x => x.Amount):N0} | Entries: {expenses.Count}</h3><hr/>");
            sb.Append("<table><tr><th>Date</th><th>Title</th><th>Category</th><th>Amount</th><th>Remark</th></tr>");
            foreach (var e in expenses.OrderByDescending(x => x.ExpenseDate))
                sb.Append($"<tr><td>{e.ExpenseDate:dd/MM/yyyy}</td><td>{e.Title}</td><td>{e.Category}</td><td>Rs {e.Amount:N0}</td><td>{e.Remark}</td></tr>");
            sb.Append("</table></body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"Expense_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }

        public static async Task<string> PrintFullReport(List<JobworkEntry> entries, List<Payment> payments, List<Expense> expenses)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} table{width:100%;border-collapse:collapse} th,td{border:1px solid #000;padding:8px} th{background:#000;color:white} h1{color:#FF6B00}</style></head><body>");
            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
                sb.Append($"<div style='text-align:center;border-bottom:2px solid #FF6B00;padding:10px'><h1>{biz.BusinessName}</h1><p>{FooterLine}</p></div>");
            sb.Append($"<h2>Full Premium Report - {DateTime.Now:dd MMM yyyy HH:mm}</h2><hr/>");

            var totalBags = entries.Sum(x => x.Bags);
            var deliveredBags = entries.Sum(x => x.DeliveredBags);
            var pendingBags = entries.Sum(x => x.PendingBags);
            var totalBilling = entries.Sum(x => x.TotalAmount);
            var totalPay = payments.Sum(x => x.Amount);
            var totalExp = expenses.Sum(x => x.Amount);

            sb.Append($"<h2>Stock: Total {totalBags} | Delivered {deliveredBags} | Pending {pendingBags} Bags</h2>");
            sb.Append($"<h2>Billing: Rs {totalBilling:N0} | Payment: Rs {totalPay:N0} | Baki: Rs {totalBilling - totalPay:N0}</h2>");
            sb.Append($"<h2>Expense: Rs {totalExp:N0} | Profit: Rs {totalBilling - totalExp:N0}</h2><hr/>");
            sb.Append("</body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"Full_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }

        private static async Task<string> GenerateBillHtml(JobworkEntry entry)
        {
            var biz = Biz;
            var sb = new StringBuilder();
            sb.Append("<html><head><style>body{font-family:Arial;padding:20px} .bill{border:3px solid #FF6B00;border-radius:15px;padding:25px;max-width:800px;margin:auto} .header{text-align:center;border-bottom:2px solid #FF6B00;padding-bottom:15px} .total{background:#FF6B00;color:white;padding:15px;border-radius:10px;text-align:center;font-size:22px;margin-top:20px} h2{color:#FF6B00;margin:5px} table{width:100%;border-collapse:collapse;margin-top:15px} th,td{border:1px solid #ddd;padding:10px;text-align:left} th{background:#f5f5f5}</style></head><body>");
            sb.Append("<div class='bill'>");

            if (!string.IsNullOrWhiteSpace(biz.BusinessName))
            {
                sb.Append($"<div class='header'><h2>{biz.BusinessName}</h2>");
                if (!string.IsNullOrWhiteSpace(biz.Address) || !string.IsNullOrWhiteSpace(biz.City))
                    sb.Append($"<p>{biz.Address} {biz.City}</p>");
                if (!string.IsNullOrWhiteSpace(biz.Mobile))
                    sb.Append($"<p>Mo: {biz.Mobile}</p>");
                if (!string.IsNullOrWhiteSpace(biz.GST))
                    sb.Append($"<p style='font-size:12px'>GST: {biz.GST}</p>");
                sb.Append("</div>");
            }
            else
            {
                sb.Append("<div class='header'><h2>BILL</h2></div>");
            }

            sb.Append($"<div style='text-align:center;margin:15px 0'><h3 style='background:#000;color:white;padding:10px;border-radius:8px;display:inline-block'>BILL NO: {entry.BillNo}</h3></div>");
            sb.Append("<table>");
            sb.Append($"<tr><th>Party Name</th><td><b>{entry.PartyName}</b></td><th>Bill Date</th><td>{entry.EntryDate:dd MMMM yyyy}</td></tr>");
            sb.Append($"<tr><th>Item Name</th><td>{entry.ItemName}</td><th>Vehicle No</th><td><b>{entry.VehicleNo}</b></td></tr>");
            sb.Append($"<tr><th>Total Bags</th><td><b>{entry.Bags} Bags</b></td><th>Bag Weight</th><td>{entry.BagWeight} KG</td></tr>");
            sb.Append($"<tr><th>Total Weight</th><td>{entry.TotalWeight:N0} KG</td><th>Bridge Weight</th><td>{entry.BridgeWeight:N0} KG</td></tr>");
            sb.Append($"<tr><th>Kharabo</th><td style='color:red'>{entry.Kharabo:N0} KG</td><th>Net Weight</th><td><b>{entry.NetWeight:N0} KG</b></td></tr>");
            sb.Append($"<tr><th>Rate Per KG</th><td>Rs {entry.RatePerKg} / KG</td><th>Status</th><td><b>{entry.Status}</b></td></tr>");
            sb.Append($"<tr><th>Pending</th><td><b style='color:red'>{entry.PendingBags} Bags</b></td><th>Delivered</th><td><b style='color:green'>{entry.DeliveredBags} Bags</b></td></tr>");
            sb.Append("</table>");
            sb.Append($"<div class='total'>TOTAL: Rs {entry.TotalAmount:N0}/- ({entry.DeliveredBags} Bags)</div>");
            sb.Append($"<p style='text-align:center;margin-top:20px;font-size:12px;color:#777'>Thank You! - Generated by {HeaderName}</p>");
            sb.Append("</div></body></html>");
            var path = Path.Combine(FileSystem.CacheDirectory, $"Bill_{entry.BillNo}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(path, sb.ToString());
            return path;
        }
    }
}