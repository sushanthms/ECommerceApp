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
        public async Task<IActionResult> UploadCsv([FromForm] IFormFile file, [FromForm] List<IFormFile>? images)
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
                var (added, updated) = await UpsertProductsAsync(csvProducts, images);

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
                .Include(p => p.Images)
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
                .Include(p => p.Images)
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
                .Include(p => p.Images)
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
                .Include(p => p.Images)
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
                .Include(p => p.Images)
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

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        // it is a function not an api endpoint.
        // processes the products from the CSV and connects the CSV image names to the actual uploaded image files.
        private async Task<(int added, int updated)> UpsertProductsAsync(List<ProductCsvDto> csvProducts, List<IFormFile>? images)
        {
            var existingProducts = await _context.Products// creating a dictionary
                .Include(p => p.Images)
                .GroupBy(p => p.SKU.ToLower())// grouping by sku and creating a dictionary
                .ToDictionaryAsync(g => g.Key, g => g.First());

            var uploadedFiles = images?// creating another dictionary. if images are null, then the next code is skipped
                .Where(f => f.Length > 0)// image size
                .ToDictionary(// The key is the filename, and the value is the actual uploaded file object.
                    f => Path.GetFileName(f.FileName).Trim().ToLower(),// here filename means image name like abc.jpg not GUID name
                    f => f
                ) ?? new Dictionary<string, IFormFile>();//Dictionary will either contain uploaded files or will be empty.

            Console.WriteLine($"Uploaded image count: {uploadedFiles.Count}");

            foreach (var fileName in uploadedFiles.Keys)// uploadedFiles.Keys contains image name(abc.png)
            {
                Console.WriteLine($"Uploaded image: {fileName}");
            }

            int added = 0;
            int updated = 0;

            foreach (var x in csvProducts)
            {
                var sku = x.SKU ?? string.Empty;
                var key = sku.ToLower();

                Product product;// product is a variable of type Product can hold one complete Product object.

                // key is the SKU stored in the dictionary. 
                // if block looks into the existingProducts dictionary and using that key it searches if same products is present in the csv file
                // if the sku in the csv matches the sku in existingProducts, then we store its reference in the variable called existing then we pass it to the variable called products
                if (existingProducts.TryGetValue(key, out var existing))
                {
                    product = existing;

                    product.SKU = sku;
                    product.Name = x.Name ?? string.Empty;
                    product.Price = x.Price;
                    product.Stock = x.Stock;
                    product.Description = x.Description ?? string.Empty;
                    product.Category = x.Category ?? string.Empty;

                    updated++;

                    // if there are images string.IsNullOrWhiteSpace(x.ImageFiles) gives false, !false makes it true and if block is executed
                    // if there are no images string.IsNullOrWhiteSpace(x.ImageFiles) gives true, !true makes it false and if block is not executed

                    if (!string.IsNullOrWhiteSpace(x.ImageFiles))
                    {
                        _context.ProductImages.RemoveRange(existing.Images);// Removing old ProductImage records
                    }
                }

                else
                {
                    product = new Product
                    {
                        SKU = sku,
                        Name = x.Name ?? string.Empty,
                        Description = x.Description ?? string.Empty,
                        Price = x.Price,
                        Stock = x.Stock,
                        Category = x.Category ?? string.Empty
                    };

                    _context.Products.Add(product);

                    existingProducts[key] = product;// key is the SKU of the new product. we already passed the products in the database to the existingProducts dictionary variable 

                    added++;
                }

                Console.WriteLine($"CSV SKU: {x.SKU}");
                Console.WriteLine($"CSV ImageFiles: {x.ImageFiles}");

                if (!string.IsNullOrWhiteSpace(x.ImageFiles))
                {
                    var imageFileNames = x.ImageFiles
                        .Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(name => name.Trim());

                    foreach (var imageFileName in imageFileNames)
                    {
                        var fileKey = imageFileName.ToLower();

                        if (!string.IsNullOrWhiteSpace(x.ImageFiles) && uploadedFiles.Count == 0)
                        {
                            throw new Exception(
                                $"CSV contains image files for SKU '{x.SKU}', but no image files were uploaded."
                            );
                        }

                        if (!uploadedFiles.TryGetValue(fileKey, out var file))
                        {
                            throw new Exception(
                                $"Image file '{imageFileName}' mentioned in the CSV was not uploaded."
                            );
                        }

                        var extension = Path.GetExtension(file.FileName)
                            .ToLowerInvariant();

                        var allowedExtensions = new[]
                        {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                        if (!allowedExtensions.Contains(extension))
                        {
                            continue;
                        }

                        var uploadFolder = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "products"
                        );

                        Directory.CreateDirectory(uploadFolder);

                        var newFileName = $"{Guid.NewGuid()}{extension}";

                        var filePath = Path.Combine(
                            uploadFolder,
                            newFileName
                        );

                        using var stream = new FileStream(
                            filePath,
                            FileMode.Create
                        );

                        await file.CopyToAsync(stream);

                        product.Images.Add(new ProductImage
                        {
                            ImageUrl = $"/images/products/{newFileName}"
                        });
                    }
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

        // runs when individual Add Product / Update Product form
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImages(int id, List<IFormFile> files)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found."
                });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Please select at least one image."
                });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var uploadFolder = Path.Combine(// here uploaded images are stored
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "products"
            );

            Directory.CreateDirectory(uploadFolder);// the directory is created if not present

            var uploadedImages = new List<ProductImage>();// uploaded images become ProductImage objects and they are stores in uploadedImages List(Array)

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        message = $"Invalid image type: {file.FileName}"
                    });
                }

                var fileName = $"{Guid.NewGuid()}{extension}";

                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);// image bytes are written to the disk.

                await file.CopyToAsync(stream);

                var image = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = $"/images/products/{fileName}"
                };

                uploadedImages.Add(image);// storing the image in the List, then for loop to the next image and stroing it in the List
            }

            if (uploadedImages.Count == 0)
            {
                return BadRequest(new
                {
                    message = "No valid images were uploaded."
                });
            }

            _context.ProductImages.AddRange(uploadedImages);// saving all images to the SQL Server

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product images uploaded successfully.",
                images = uploadedImages.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.ImageUrl
                })
            });
        }
    }
}

