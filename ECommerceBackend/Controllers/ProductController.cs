using CsvHelper;
using CsvHelper.Configuration;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

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
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim
                });

                var csvProducts = csv.GetRecords<ProductCsvDto>().ToList();

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct(ProductCsvDto dto)
        {
            var product = new Product
            {
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

            product.Name = dto.Name ?? string.Empty;
            product.Description = dto.Description ?? string.Empty;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Category = dto.Category ?? string.Empty;
            product.ImageUrl = dto.ImageUrl ?? string.Empty;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // Shared by the CSV upload endpoint. Matches by Name (case-insensitive):
        // existing product = update its fields, new name = insert a new row.
        public async Task<(int added, int updated)> UpsertProductsAsync(List<ProductCsvDto> csvProducts)
        {
            var existingProducts = await _context.Products
                .GroupBy(p => p.Name.ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.First());

            int added = 0, updated = 0;

            foreach (var x in csvProducts)
            {
                var name = x.Name ?? string.Empty;
                var key = name.ToLower();

                if (existingProducts.TryGetValue(key, out var existing))
                {
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
                        Name = name,
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