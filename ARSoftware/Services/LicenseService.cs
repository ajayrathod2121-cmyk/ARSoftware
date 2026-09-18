using System.Security.Cryptography;
using System.Text;
using ARSoftware.Models;

namespace ARSoftware.Services
{
    public class LicenseService
    {
        private const string SECRET = "AR_TRADING_BHAVNAGAR_2024_SECRET_KEY_123!";

        public string GenerateKey(string type)
        {
            DateTime expiry = type switch
            {
                "DEMO" => DateTime.Now.AddDays(7),
                "1M" => DateTime.Now.AddMonths(1),
                "6M" => DateTime.Now.AddMonths(6),
                "1Y" => DateTime.Now.AddYears(1),
                "2Y" => DateTime.Now.AddYears(2),
                "3Y" => DateTime.Now.AddYears(3),
                _ => DateTime.Now.AddDays(7)
            };
            string raw = $"{type}|{expiry:yyyy-MM-dd}|{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            string encrypted = Encrypt(raw);
            return $"AR-{type}-{encrypted.Substring(0, 16).ToUpper()}-{encrypted.Substring(16, 8).ToUpper()}";
        }

        public LicenseInfo ValidateKey(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return null;
                if (!key.StartsWith("AR-")) return null;
                var parts = key.Split('-');
                if (parts.Length < 4) return null;
                string type = parts[1];
                DateTime expiry = Preferences.Get("LicenseExpiry", DateTime.MinValue);
                if (expiry == DateTime.MinValue)
                {
                    expiry = type switch
                    {
                        "DEMO" => DateTime.Now.AddDays(7),
                        "1M" => DateTime.Now.AddMonths(1),
                        "6M" => DateTime.Now.AddMonths(6),
                        "1Y" => DateTime.Now.AddYears(1),
                        "2Y" => DateTime.Now.AddYears(2),
                        "3Y" => DateTime.Now.AddYears(3),
                        _ => DateTime.Now.AddDays(1)
                    };
                }
                return new LicenseInfo
                {
                    Key = key,
                    Type = type,
                    ExpiryDate = expiry,
                    IsActive = DateTime.Now <= expiry
                };
            }
            catch { return null; }
        }

        public bool IsLicenseValid()
        {
            string key = Preferences.Get("LicenseKey", "");
            if (string.IsNullOrEmpty(key)) return false;
            var info = ValidateKey(key);
            if (info == null) return false;
            if (!info.IsActive) return false;
            return true;
        }

        public void SaveLicense(string key)
        {
            var parts = key.Split('-');
            string type = parts[1];
            DateTime expiry = type switch
            {
                "DEMO" => DateTime.Now.AddDays(7),
                "1M" => DateTime.Now.AddMonths(1),
                "6M" => DateTime.Now.AddMonths(6),
                "1Y" => DateTime.Now.AddYears(1),
                "2Y" => DateTime.Now.AddYears(2),
                "3Y" => DateTime.Now.AddYears(3),
                _ => DateTime.Now.AddDays(7)
            };
            Preferences.Set("LicenseKey", key);
            Preferences.Set("LicenseType", type);
            Preferences.Set("LicenseExpiry", expiry);
            Preferences.Set("LicenseActivatedOn", DateTime.Now);
        }

        private string Encrypt(string text)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(SECRET.Substring(0, 32));
            aes.IV = new byte[16];
            var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(text);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted).Replace("=", "").Replace("+", "").Replace("/", "");
        }

        public Dictionary<string, string> GetAllDemoKeys()
        {
            return new Dictionary<string, string>
            {
                { "DEMO (7 Days)", GenerateKey("DEMO") },
                { "1 Month", GenerateKey("1M") },
                { "6 Month", GenerateKey("6M") },
                { "1 Year", GenerateKey("1Y") },
                { "2 Years", GenerateKey("2Y") },
                { "3 Years", GenerateKey("3Y") },
            };
        }
    }
}