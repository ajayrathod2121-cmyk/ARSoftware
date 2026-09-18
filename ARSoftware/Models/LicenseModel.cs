namespace ARSoftware.Models
{
    public class LicenseInfo
    {
        public string Key { get; set; }
        public string Type { get; set; } // DEMO, 1M, 6M, 1Y, 2Y, 3Y
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysLeft => (ExpiryDate - DateTime.Now).Days;
    }
}