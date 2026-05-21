namespace NearGo.Models
{
    public class PlatformFee
    {
        public int Id { get; set; }
        public int SupermarketId { get; set; }
        public string FeeType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public Supermarket Supermarket { get; set; } = null!;
    }
}
