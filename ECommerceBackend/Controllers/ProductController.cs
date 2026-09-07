using CsvHelper;
using CsvHelper.Configuration;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// IActionResult
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ECommerceBackend.Controllers
{
    [ApiController]// it gives the next line feature
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        // POST /api/Product/upload
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a CSV file.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".csv")
            {
                return BadRequest("Only CSV files are allowed.");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());

                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,// disables CsvHelper's default behavior of throwing an exception if the CSV headers don't exactly match the DTO's property names
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim
                });

                var csvProducts = csv.GetRecords<ProductCsvDto>().ToList();
                // gives the data to UpsertProductsAsync fucntion
                var (added, updated) = await UpsertProductsAsync(csvProducts);

                return Ok(new
                {
                    message = $"{added} products added, {updated} products updated.",
                    added,
                    updated
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return BadRequest(new
                {
                    message = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return await GetProducts();
            }

            search = search.ToLower();

            var products = await _context.Products
                .Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Category.ToLower().Contains(search))
                .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct(ProductCsvDto dto)
        {
            var sku = dto.SKU?.Replace(" ", "").Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sku))
            {
                return BadRequest(new
                {
                    message = "SKU is required."
                });
            }
            var skuExists = await _context.Products
        .AnyAsync(p => p.SKU.Trim().ToLower() == sku.ToLower());

            if (skuExists)
            {
                return BadRequest(new
                {
                    message = "This SKU already exists."
                });
            }

            var product = new Product
            {
                SKU = dto.SKU ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category ?? string.Empty,
                ImageUrl = dto.ImageUrl ?? string.Empty
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCsvDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var sku = dto.SKU?.Replace(" ", "").Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sku))
            {
                return BadRequest(new
                {
                    message = "SKU is required."
                });
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU.Trim().ToLower() == sku.ToLower() && p.Id != id);// the product from the for loop and the product we are updating
            //  p.Id != id means in for loop we get a product which is the product we are updating, so we have to skip it.
            // excludes the product we're currently editing from the check.
            if (skuExists)
            {
                return BadRequest(new
                {
                    message = "This SKU already exists."
                });
            }

            product.SKU = sku;
            product.Name = dto.Name ?? string.Empty;
            product.Description = dto.Description ?? string.Empty;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Category = dto.Category ?? string.Empty;
            product.ImageUrl = dto.ImageUrl ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        // it is a function not an api endpoint
        public async Task<(int added, int updated)> UpsertProductsAsync(List<ProductCsvDto> csvProducts)
        {
            var existingProducts = await _context.Products
                .GroupBy(p => p.SKU.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First());

            int added = 0, updated = 0;

            foreach (var x in csvProducts)
            {
                var sku = x.SKU ?? string.Empty;
                var key = sku.ToLower();
                // key is the the current product name stored in the dictionary. 
                // if block looks into the existingProducts dictionary and usiing that key it searches if same products is present in the csv file
                if (existingProducts.TryGetValue(key, out var existing))
                {
                    existing.SKU = sku;
                    existing.Name = x.Name ?? string.Empty;
                    existing.Price = x.Price;
                    existing.Stock = x.Stock;
                    existing.Description = x.Description ?? string.Empty;
                    existing.Category = x.Category ?? string.Empty;
                    existing.ImageUrl = x.ImageUrl ?? string.Empty;

                    updated++;
                }
                else
                {
                    var newProduct = new Product
                    {
                        SKU = sku,
                        Name = x.Name ?? string.Empty,
                        Description = x.Description ?? string.Empty,
                        Price = x.Price,
                        Stock = x.Stock,
                        Category = x.Category ?? string.Empty,
                        ImageUrl = x.ImageUrl ?? string.Empty
                    };

                    _context.Products.Add(newProduct);
                    existingProducts[key] = newProduct;
                    added++;
                }
            }

            await _context.SaveChangesAsync();

            return (added, updated);
        }
    }
}