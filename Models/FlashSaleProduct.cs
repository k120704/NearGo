namespace NearGo.Models
{
    public class FlashSaleProduct
    {
        public int Id { get; set; }
        public int FlashSaleId { get; set; }
        public int ProductId { get; set; }
        public decimal SalePrice { get; set; }
        public int MaxQuantity { get; set; }
        public int SoldQuantity { get; set; }
        public FlashSale FlashSale { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
