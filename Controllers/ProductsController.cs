using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductAPI.Models;
using ProductAPI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        // Constructor to initialize the database context and logger
        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all products from the database.
        /// </summary>
        /// <returns>A list of products.</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Fetching all products from the database.");
            try
            {
                var products = await _context.Products.ToListAsync();
                _logger.LogInformation("Successfully retrieved {Count} products.", products.Count);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching products.");
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Retrieves a specific product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>The requested product or a 404 if not found.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInformation("Fetching product with ID {Id}.", id);
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved product with ID {Id}.", id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Creates a new product in the database.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The created product with its unique ID.</returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _logger.LogInformation("Creating a new product.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product model state.");
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                product.Id = await GenerateUniqueIdAsync();
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product created successfully with ID {Id}.", product.Id);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred while creating a product.");
                return StatusCode(500, string.Format(Constants.DatabaseUpdateError, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a product.");
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="updatedProduct">The updated product details.</param>
        /// <returns>No content if successful, or an error if not.</returns>
        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
        {
            _logger.LogInformation("Updating product with ID {Id}.", id);
            if (id != updatedProduct.Id)
            {
                _logger.LogWarning("Product ID mismatch. Provided ID: {Id}, Product ID: {ProductId}.", id, updatedProduct.Id);
                return BadRequest(string.Format(Constants.ProductIdMismatch));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product model state.");
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                _context.Entry(updatedProduct).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product with ID {Id} updated successfully.", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "A concurrency error occurred while updating product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.ConcurrencyError, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>No content if successful, or an error if not.</returns>
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with ID {Id}.", id);
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product with ID {Id} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Decrements the stock of a product by a specified quantity.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="quantity">The quantity to decrement.</param>
        /// <returns>OK if successful, or an error if not.</returns>
        [AllowAnonymous]
        [HttpPut("decrement-stock/{id}/{quantity}")]
        public async Task<IActionResult> DecrementStock(int id, int quantity)
        {
            _logger.LogInformation("Decrementing stock for product with ID {Id} by {Quantity}.", id, quantity);
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                if (product.StockAvailable < quantity)
                {
                    _logger.LogWarning("Insufficient stock for product with ID {Id}. Available: {Stock}, Requested: {Quantity}.", id, product.StockAvailable, quantity);
                    return BadRequest(string.Format(Constants.NotEnoughStock));
                   
                }

                product.StockAvailable -= quantity;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock decremented successfully for product with ID {Id}. New stock: {Stock}.", id, product.StockAvailable);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while decrementing stock for product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Increments the stock of a product by a specified quantity.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>OK if successful, or an error if not.</returns>
        [AllowAnonymous]
        [HttpPut("add-to-stock/{id}/{quantity}")]
        public async Task<IActionResult> AddToStock(int id, int quantity)
        {
            _logger.LogInformation("Adding stock for product with ID {Id} by {Quantity}.", id, quantity);
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", id);
                    return NotFound();
                }

                product.StockAvailable += quantity;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock added successfully for product with ID {Id}. New stock: {Stock}.", id, product.StockAvailable);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding stock for product with ID {Id}.", id);
                return StatusCode(500, string.Format(Constants.InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Generates a unique 6-digit ID for a product.
        /// </summary>
        /// <returns>A unique 6-digit ID.</returns>
        private async Task<int> GenerateUniqueIdAsync()
        {
            _logger.LogInformation("Generating a unique 6-digit ID for a new product.");
            try
            {
                int newId;
                var random = new Random();
                do
                {
                    newId = random.Next(100000, 999999);
                }
                while (await _context.Products.AnyAsync(p => p.Id == newId));

                _logger.LogInformation("Generated unique ID: {Id}.", newId);
                return newId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating a unique ID.");
                throw new Exception(string.Format(Constants.UniqueIDError, ex.Message));
            }
        }
    }
}
