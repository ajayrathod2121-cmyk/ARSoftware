namespace ARSoftware.Services
{
    public class BusinessProfile
    {
        public string BusinessName { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "Bhavnagar";
        public string Pincode { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Email { get; set; } = "";
        public string GST { get; set; } = "";
        public string BankName { get; set; } = "";
        public string AccountNo { get; set; } = "";
        public string IFSC { get; set; } = "";
        public string AccountHolder { get; set; } = "";
        public string UPI { get; set; } = "";
        public string Terms { get; set; } = "";
        public string LogoPath { get; set; } = "";
        public string QrPath { get; set; } = "";
    }

    public static class BusinessService
    {
        public static void SaveBusiness(BusinessProfile p)
        {
            Preferences.Set("biz_name", p.BusinessName ?? "");
            Preferences.Set("biz_address", p.Address ?? "");
            Preferences.Set("biz_city", p.City ?? "Bhavnagar");
            Preferences.Set("biz_pincode", p.Pincode ?? "");
            Preferences.Set("biz_mobile", p.Mobile ?? "");
            Preferences.Set("biz_email", p.Email ?? "");
            Preferences.Set("biz_gst", p.GST ?? "");
            Preferences.Set("biz_bank", p.BankName ?? "");
            Preferences.Set("biz_accno", p.AccountNo ?? "");
            Preferences.Set("biz_ifsc", p.IFSC ?? "");
            Preferences.Set("biz_holder", p.AccountHolder ?? "");
            Preferences.Set("biz_upi", p.UPI ?? "");
            Preferences.Set("biz_terms", p.Terms ?? "");
            Preferences.Set("biz_logo", p.LogoPath ?? "");
            Preferences.Set("biz_qr", p.QrPath ?? "");
        }

        public static BusinessProfile GetBusiness()
        {
            return new BusinessProfile
            {
                BusinessName = Preferences.Get("biz_name", "AR SOFTWARE"),
                Address = Preferences.Get("biz_address", ""),
                City = Preferences.Get("biz_city", "Bhavnagar"),
                Pincode = Preferences.Get("biz_pincode", ""),
                Mobile = Preferences.Get("biz_mobile", ""),
                Email = Preferences.Get("biz_email", ""),
                GST = Preferences.Get("biz_gst", ""),
                BankName = Preferences.Get("biz_bank", ""),
                AccountNo = Preferences.Get("biz_accno", ""),
                IFSC = Preferences.Get("biz_ifsc", ""),
                AccountHolder = Preferences.Get("biz_holder", ""),
                UPI = Preferences.Get("biz_upi", ""),
                Terms = Preferences.Get("biz_terms", ""),
                LogoPath = Preferences.Get("biz_logo", ""),
                QrPath = Preferences.Get("biz_qr", "")
            };
        }
    }
}