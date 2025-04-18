using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace ProductAPI.Models
{
    public class Product
    {
        public int Id { get; set; } // 6-digit unique ID

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Name can only contain letters, numbers, and spaces.")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "StockAvailable must be greater than 0.")]
        public int StockAvailable { get; set; }
    }
}
