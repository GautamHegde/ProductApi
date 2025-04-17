namespace ProductAPI.Models
{
    public class Product
    {
        public int Id { get; set; } // 6-digit unique ID
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockAvailable { get; set; }
    }
}
