namespace NearGo.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int SupermarketId { get; set; }
        public string Tier { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Supermarket Supermarket { get; set; } = null!;
    }
}
