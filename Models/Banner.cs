namespace NearGo.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int PackageDays { get; set; } = 30;
        public decimal PackagePrice { get; set; } = 200000;
        public string? LinkUrl { get; set; }
        public string? ButtonText { get; set; }
        public int? SupermarketId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Pending";
        public string? PaymentStatus { get; set; } = "Unpaid";
        public string? TransactionId { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Supermarket? Supermarket { get; set; }
    }
}
