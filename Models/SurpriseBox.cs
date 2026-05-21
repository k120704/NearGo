namespace NearGo.Models
{
    public class SurpriseBox
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalValue { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<SurpriseBoxProduct> SurpriseBoxProducts { get; set; } = new List<SurpriseBoxProduct>();
    }
}
