using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductAPI.Models;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new();

        // Constructor to initialize the database context
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all products from the database.
        /// </summary>
        /// <returns>A list of products.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                return await _context.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a specific product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The requested product or a 404 if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                return product;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new product in the database.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The created product with its unique ID.</returns>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                product.Id = await GenerateUniqueIdAsync();
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database update error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="updatedProduct">The updated product details.</param>
        /// <returns>No content if successful, or an error if not.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
        {
            if (id != updatedProduct.Id)
                return BadRequest("Product ID mismatch");

            try
            {
                _context.Entry(updatedProduct).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return StatusCode(500, $"Concurrency error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>No content if successful, or an error if not.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound();

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrements the stock of a product by a specified quantity.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="quantity">The quantity to decrement.</param>
        /// <returns>OK if successful, or an error if not.</returns>
        [HttpPut("decrement-stock/{id}/{quantity}")]
        public async Task<IActionResult> DecrementStock(int id, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                if (product.StockAvailable < quantity)
                    return BadRequest("Not enough stock");

                product.StockAvailable -= quantity;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Increments the stock of a product by a specified quantity.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>OK if successful, or an error if not.</returns>
        [HttpPut("add-to-stock/{id}/{quantity}")]
        public async Task<IActionResult> AddToStock(int id, int quantity)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                product.StockAvailable += quantity;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a unique 6-digit ID for a product.
        /// </summary>
        /// <returns>A unique 6-digit ID.</returns>
        private async Task<int> GenerateUniqueIdAsync()
        {
            try
            {
                int newId;
                do
                {
                    newId = _random.Next(100000, 999999);
                }
                while (await _context.Products.AnyAsync(p => p.Id == newId));

                return newId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating unique ID: {ex.Message}");
            }
        }
    }
}
