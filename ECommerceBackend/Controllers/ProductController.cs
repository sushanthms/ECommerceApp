using CsvHelper;
using CsvHelper.Configuration;
using ECommerceBackend.Controllers;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// IActionResult
using Microsoft.EntityFrameworkCore;
using System.Globalization;
// AppDbContext is created in program.cs

// private readonly AppDbContext _context;
// _context is a variable of type AppDbContext object. _context object is not yet created. _context is a variable now.
// Creates a field called _context that will hold an AppDbContext object.  
// public ProductController(AppDbContext context)
// context is of type AppDbContext.context is the name used while receiving.  
// _context is where the controller keeps the reference so it can use it later.  
// _context = context;
// Takes the AppDbContext object we received through context and stores it in _context.  
// context receives a reference to an actual AppDbContext object. It does NOT receive the data inside the object, and it does not become a copy of the object.
// readonly means the field can be assigned during initialization/constructor execution but shouldn't be reassigned afterward. later we use it by writing _context.Users
namespace ECommerceBackend.Controllers
{
    [ApiController]// it gives the next line feature
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // AuthController depends on AppDbContext, and ASP.NET Core's DI container injects an AppDbContext object into the AuthController constructor.
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
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 20, string category = "")
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            var query = _context.Products.Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categories = category
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    categories.Contains(p.Category.ToLower()));
            }

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize
            );

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                products,
                page,
                pageSize,
                totalProducts,
                totalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllProductsForAdmin(int page = 1, int pageSize = 20)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            var query = _context.Products;

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize
            );

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                products,
                page,
                pageSize,
                totalProducts,
                totalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string search, int page = 1, int pageSize = 20, string category = "")
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            var query = _context.Products
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Category.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categories = category
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToLower())
                    .ToList();

                query = query.Where(p =>
                    categories.Contains(p.Category.ToLower()));
            }

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize
            );

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                products,
                page,
                pageSize,
                totalProducts,
                totalPages
            });
        }

        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchProductsForAdmin(string search, int page = 1, int pageSize = 20)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1 || pageSize > 100)
            {
                pageSize = 20;
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                return await GetAllProductsForAdmin(page, pageSize);
            }

            search = search.Trim().ToLower();

            var query = _context.Products
    .Where(p =>
        p.Name.ToLower().Contains(search) ||
        p.Category.ToLower().Contains(search) ||
        p.SKU.ToLower().Contains(search));

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize
            );

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                products,
                page,
                pageSize,
                totalProducts,
                totalPages
            });
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

        [HttpPut("{id}/hide")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HideProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product hidden successfully" });
        }

        [HttpPut("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsDeleted = false;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product made visible successfully" });
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PermanentlyDeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product permanently deleted" });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Products
                .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }
    }
}