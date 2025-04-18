using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductApi.Data;
using ProductAPI.Controllers;
using ProductAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductAPI.Tests
{
    public class ProductsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            // Configure an in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize the database context
            _context = new AppDbContext(options);

            // Create a mock logger
            var mockLogger = new Mock<ILogger<ProductsController>>();

            // Initialize the controller with the context and mock logger
            _controller = new ProductsController(_context, mockLogger.Object);

            // Seed the database with initial test data
            SeedDatabase();
        }


        // Method to seed the in-memory database with test data
        private void SeedDatabase()
        {
            if (!_context.Products.Any())
            {
                _context.Products.AddRange(
                    new Product { Id = 100001, Name = "Product1", Description = "Description1", Price = 10.0m, StockAvailable = 100 },
                    new Product { Id = 100002, Name = "Product2", Description = "Description2", Price = 20.0m, StockAvailable = 200 }
                );
                _context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            // Act: Call the GetProducts method
            var result = await _controller.GetProducts();

            // Assert: Verify the result contains all products
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            Assert.NotNull(actionResult.Value); // Ensure the result is not null
            var products = Assert.IsType<List<Product>>(actionResult.Value);
            Assert.Equal(2, products.Count); // Verify the count matches the seeded data
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenProductExists()
        {
            // Act: Call the GetProduct method with an existing product ID
            var result = await _controller.GetProduct(100001);

            // Assert: Verify the returned product matches the expected data
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.NotNull(actionResult.Value); // Ensure the result is not null
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal(100001, product.Id); // Verify the product ID matches
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Act: Call the GetProduct method with a non-existent product ID
            var result = await _controller.GetProduct(999999);

            // Assert: Verify the result is a NotFound response
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_AddsProductToDatabase()
        {
            // Arrange: Create a new product to add
            var newProduct = new Product
            {
                Name = "NewProduct",
                Description = "NewDescription",
                Price = 30.0m,
                StockAvailable = 300
            };

            // Act: Call the CreateProduct method
            var result = await _controller.CreateProduct(newProduct);

            // Assert: Verify the product was added successfully
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(actionResult.Value); // Ensure the result is not null
            var createdProduct = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal("NewProduct", createdProduct.Name); // Verify the product name matches
            Assert.Equal(2, _context.Products.Count()); // Verify the total product count
        }

        [Fact]
        public async Task UpdateProduct_UpdatesExistingProduct()
        {
            // Arrange: Create an updated product object
            var updatedProduct = new Product
            {
                Id = 100001,
                Name = "UpdatedProduct",
                Description = "UpdatedDescription",
                Price = 15.0m,
                StockAvailable = 150
            };

            // Act: Call the UpdateProduct method
            var result = await _controller.UpdateProduct(100001, updatedProduct);

            // Assert: Verify the product was updated successfully
            Assert.IsType<NoContentResult>(result);
            var product = await _context.Products.FindAsync(100001);
            Assert.NotNull(product); // Ensure the product exists
            Assert.Equal("UpdatedProduct", product.Name); // Verify the product name was updated
        }

        [Fact]
        public async Task UpdateProduct_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange: Create a product with a mismatched ID
            var updatedProduct = new Product
            {
                Id = 100002,
                Name = "UpdatedProduct",
                Description = "UpdatedDescription",
                Price = 15.0m,
                StockAvailable = 150
            };

            // Act: Call the UpdateProduct method with a mismatched ID
            var result = await _controller.UpdateProduct(100001, updatedProduct);

            // Assert: Verify the result is a BadRequest response
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProductFromDatabase()
        {
            // Act: Call the DeleteProduct method
            var result = await _controller.DeleteProduct(100001);

            // Assert: Verify the product was removed successfully
            Assert.IsType<NoContentResult>(result);
            var product = await _context.Products.FindAsync(100001);
            Assert.Null(product); // Ensure the product no longer exists
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Act: Call the DeleteProduct method with a non-existent product ID
            var result = await _controller.DeleteProduct(999999);

            // Assert: Verify the result is a NotFound response
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DecrementStock_DecreasesStock_WhenSufficientStockExists()
        {
            // Act: Call the DecrementStock method
            var result = await _controller.DecrementStock(100001, 10);

            // Assert: Verify the stock was decremented successfully
            Assert.IsType<OkResult>(result);
            var product = await _context.Products.FindAsync(100001);
            Assert.NotNull(product); // Ensure the product exists
            Assert.Equal(140, product.StockAvailable); // Verify the stock was decremented
        }

        [Fact]
        public async Task DecrementStock_ReturnsBadRequest_WhenInsufficientStock()
        {
            // Act: Call the DecrementStock method with a quantity greater than available stock
            var result = await _controller.DecrementStock(100001, 200);

            // Assert: Verify the result is a BadRequest response
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddToStock_IncreasesStock()
        {
            // Act: Call the AddToStock method
            var result = await _controller.AddToStock(100001, 50);

            // Assert: Verify the stock was incremented successfully
            Assert.IsType<OkResult>(result);
            var product = await _context.Products.FindAsync(100001);
            Assert.NotNull(product); // Ensure the product exists
            Assert.Equal(190, product.StockAvailable); // Verify the stock was incremented
        }
    }
}
