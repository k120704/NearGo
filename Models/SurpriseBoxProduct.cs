namespace NearGo.Models
{
    public class SurpriseBoxProduct
    {
        public int Id { get; set; }
        public int SurpriseBoxId { get; set; }
        public int ProductId { get; set; }
        public SurpriseBox SurpriseBox { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
