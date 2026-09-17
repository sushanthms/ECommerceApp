using CsvHelper;
using CsvHelper.Configuration;
using ECommerceBackend.Data;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ECommerceBackend.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;
        private readonly IApplicationLogger _logger;

        public ProductService(AppDbContext context, IApplicationLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get products
        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> GetProductsAsync(int page, int pageSize, string category)
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

            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = await query
                .Include(p => p.Images)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        // Get one product
        public async Task<Product?> GetProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return product;
        }

        // Get all products for admin
        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> GetAllProductsForAdminAsync(
            int page,
            int pageSize)
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

            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = await query
                .Include(p => p.Images)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        // Search products
        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> SearchProductsAsync(string search, int page, int pageSize, string category)
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

            return (products, totalProducts, totalPages);
        }

        // Search products for admin
        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> SearchProductsForAdminAsync(
            string search,
            int page,
            int pageSize)
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
                return await GetAllProductsForAdminAsync(page, pageSize);
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

            return (products, totalProducts, totalPages);
        }

        // Add product
        public async Task<(Product? Product, string? Error)> AddProductAsync(ProductCsvDto dto)
        {
            var sku = dto.SKU?.Replace(" ", "").Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sku))
            {
                return (null, "SKU is required.");
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU.Trim().ToLower() == sku.ToLower());

            if (skuExists)
            {
                return (null, "This SKU already exists.");
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

            return (product, null);
        }

        // Update product
        public async Task<(Product? Product, string? Error, bool NotFound)> UpdateProductAsync(
            int id,
            ProductCsvDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return (null, null, true);
            }

            var sku = dto.SKU?.Replace(" ", "").Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sku))
            {
                return (null, "SKU is required.", false);
            }

            var skuExists = await _context.Products
                .AnyAsync(p =>
                    p.SKU.Trim().ToLower() == sku.ToLower() &&
                    p.Id != id);

            if (skuExists)
            {
                return (null, "This SKU already exists.", false);
            }

            product.SKU = sku;
            product.Name = dto.Name ?? string.Empty;
            product.Description = dto.Description ?? string.Empty;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Category = dto.Category ?? string.Empty;

            await _context.SaveChangesAsync();

            return (product, null, false);
        }

// Reads the CSV file and processes the products
public async Task<(int added, int updated)> UploadCsvAsync(IFormFile file, List<IFormFile>? images)
        {
            await _logger.LogMessageAsync("CSV product upload started.");
            using var reader = new StreamReader(file.OpenReadStream());

            using var csv = new CsvReader(
                reader,
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,// disables CsvHelper's default behavior of throwing an exception if the CSV headers don't exactly match the DTO's property names
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim
                });

            var csvProducts = csv
                .GetRecords<ProductCsvDto>()
                .ToList();

            // gives the data to UpsertProductsAsync function
            var (added, updated) = await UpsertProductsAsync(csvProducts, images);
            await _logger.LogMessageAsync($"CSV product upload completed. Added: {added}, Updated: {updated}");

            return (added, updated);
        }

        // processes the products from the CSV and connects the CSV image names to the actual uploaded image files.
        public async Task<(int added, int updated)> UpsertProductsAsync(List<ProductCsvDto> csvProducts, List<IFormFile>? images)
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

        // Hide product
        public async Task<bool> HideProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            return true;
        }

        // Restore product
        public async Task<bool> RestoreProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            product.IsDeleted = false;

            await _context.SaveChangesAsync();

            return true;
        }

        // Permanently delete product
        public async Task<bool> PermanentlyDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return true;
        }

        // Get categories
        public async Task<List<string>> GetCategoriesAsync()
        {
            var categories = await _context.Products
                .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories;
        }

        // Upload product images
        public async Task<(bool Success, string? Error, List<ProductImage>? Images)> UploadProductImagesAsync(
            int id,
            List<IFormFile> files)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return (false, "Product not found.", null);
            }

            if (files == null || files.Count == 0)
            {
                return (false, "Please select at least one image.", null);
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

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
                    return (false, $"Invalid image type: {file.FileName}", null);
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
                return (false, "No valid images were uploaded.", null);
            }

            _context.ProductImages.AddRange(uploadedImages);// saving all images to the SQL Server

            await _context.SaveChangesAsync();

            return (true, null, uploadedImages);
        }
    }
}