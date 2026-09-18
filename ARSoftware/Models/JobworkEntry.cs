namespace ARSoftware.Models
{
    public class JobworkEntry
    {
        public long Id { get; set; }
        public string BillNo { get; set; } = "";
        public long PartyId { get; set; }
        public long ItemId { get; set; }
        public string PartyName { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int Bags { get; set; }
        public decimal BagWeight { get; set; } = 40;
        public decimal TotalWeight { get; set; }
        public decimal BridgeWeight { get; set; }
        public decimal Kharabo { get; set; }
        public decimal NetWeight { get; set; }
        public string VehicleNo { get; set; } = "";
        public decimal RatePerKg { get; set; } = 50;
        public decimal TotalAmount { get; set; }
        public int PendingBags { get; set; }
        public int DeliveredBags { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime EntryDate { get; set; } = DateTime.Now.Date;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Premium Dashboard માટે Extra - તારો જૂનો Code તૂટશે નહીં
        public string DisplayInfo => $"{ItemName} • {PendingBags} Baki • Rs {TotalAmount:N0}";
        public string DisplayBags => $"{Bags} Bags";
        public string MasalaType => ItemName;
    }
}