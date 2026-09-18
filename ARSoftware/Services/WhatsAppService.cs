using System.Linq;

namespace ARSoftware.Services
{
    public class WhatsAppService
    {
        private static string CleanNumber(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return "";
            string clean = new string(mobile.Where(char.IsDigit).ToArray());
            if (clean.Length == 10) clean = "91" + clean;
            return clean;
        }

        // ✅ બધા Function હવે static - new વગર Call થશે!
        public static async Task SendDeliveryMessage(string mobile, string partyName, string itemName, object qty)
        {
            string num = CleanNumber(mobile);
            if (string.IsNullOrEmpty(num)) return;
            string msg = $"*AR SOFTWARE - Delivery*%0A%0Aનમસ્તે {partyName},%0Aતમારું *{itemName}* - *{qty} Bags* Deliver થઈ ગયું છે.%0A%0AAR Software 🚚";
            await Launcher.OpenAsync($"https://wa.me/{num}?text={msg}");
        }

        // ✅ તમારી Line 181 માટે - 9 Argument - static!
        public static async Task SendDeliveryMessage(string mobile, string partyName, string itemName, object qty, object arg5, object arg6, object arg7, object arg8, object arg9)
        {
            try
            {
                string num = CleanNumber(mobile);
                if (string.IsNullOrEmpty(num)) return;

                string q5 = arg5?.ToString() ?? "";
                string q6 = arg6?.ToString() ?? "";
                string q7 = arg7?.ToString() ?? "";
                string q8 = arg8 is DateTime dt ? dt.ToString("dd-MM-yyyy") : arg8?.ToString() ?? "";
                string q9 = arg9?.ToString() ?? "";

                string msg = $"*AR SOFTWARE - Javak*%0A%0A" +
                             $"નમસ્તે {partyName},%0A%0A" +
                             $"📦 Item: *{itemName}*%0A" +
                             $"🔢 Qty: *{qty}*%0A" +
                             $"🧾 No: *{q5}*%0A" +
                             $"💰 Rate: *{q6}*%0A" +
                             $"🚛 Detail: *{q7}*%0A" +
                             $"📅 Date: *{q8}*%0A" +
                             $"💵 Total: *Rs {q9}*%0A%0A" +
                             $"Deliver થઈ ગયું છે!%0A%0Aઆભાર! 🙏%0AAR Software";

                await Launcher.OpenAsync($"https://wa.me/{num}?text={msg}");
            }
            catch { }
        }

        public static async Task SendBillAsync(string mobile, string partyName, string billNo, object amount)
        {
            string num = CleanNumber(mobile);
            if (string.IsNullOrEmpty(num)) return;
            string msg = $"*AR SOFTWARE - Rajkot*%0A%0Aનમસ્તે {partyName},%0Aતમારું Bill No: *{billNo}*%0AAmount: *Rs {amount}*%0A%0Aઆભાર! 🙏";
            await Launcher.OpenAsync($"https://wa.me/{num}?text={msg}");
        }
    }
}