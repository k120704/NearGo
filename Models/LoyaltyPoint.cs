namespace NearGo.Models
{
    public class LoyaltyPoint
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Points { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
