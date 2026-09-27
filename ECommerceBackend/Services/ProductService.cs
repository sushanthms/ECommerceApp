using CsvHelper;
using CsvHelper.Configuration;
using ECommerceBackend.Data;
using ECommerceBackend.DTOs;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace ECommerceBackend.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;
        private readonly IApplicationLogger _logger;
        private readonly IMemoryCache _cache;

        private const string CategoriesCacheKey = "product_categories";

        public ProductService(AppDbContext context, IApplicationLogger logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<(List<ProductListDto> Products, int TotalProducts, int TotalPages)> GetProductsAsync(int page, int pageSize, string category)
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
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categories = category// creating a list of categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

                query = query.Where(p =>
                    categories.Contains(p.Category));
            }

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Category = p.Category,

                    Images = p.Images
                    .Select(i => new ProductImageDto
                    {
                        ImageUrl = i.ImageUrl
                    })
                    .ToList()
                })
            .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            // throw new Exception("Testing global exception handler");

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return product;
        }

        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> GetAllProductsForAdminAsync(int page, int pageSize)
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
                .AsNoTracking()
                .Include(p => p.Images)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        public async Task<(List<ProductListDto> Products, int TotalProducts, int TotalPages)> SearchProductsAsync(string search, int page, int pageSize, string category)
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
                .AsNoTracking()
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Category.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categories = category
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

                query = query.Where(p =>
                    categories.Contains(p.Category));
            }

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                (double)totalProducts / pageSize
            );

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    Category = p.Category,

                    Images = p.Images
                        .Select(i => new ProductImageDto
                        {
                            ImageUrl = i.ImageUrl
                        })
                        .ToList()
                })
                .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        public async Task<(List<Product> Products, int TotalProducts, int TotalPages)> SearchProductsForAdminAsync(string search, int page, int pageSize)
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
                    p.Name.Contains(search) ||
                    p.Category.Contains(search) ||
                    p.SKU.Contains(search));

            var totalProducts = await query.CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = await query
                .AsNoTracking()
                .Include(p => p.Images)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalProducts, totalPages);
        }

        public async Task<(Product? Product, string? Error)> AddProductAsync(ProductCsvDto dto)
        {
            var sku = dto.SKU?.Replace(" ", "").Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sku))
            {
                return (null, "SKU is required.");
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU == sku);

            if (skuExists)
            {
                return (null, "This SKU already exists.");
            }

            if (dto.Price < 0 || dto.Stock < 0)
            {
                return (null, "Price and stock cannot be negative.");
            }

            var product = new Product
            {
                SKU = sku,
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                Stock = dto.Stock,
                Category = dto.Category ?? string.Empty,
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            await _logger.LogMessageAsync($"Product added - ProductId: {product.Id}, SKU: {product.SKU}");
            return (product, null);
        }

        public async Task<(Product? Product, string? Error, bool NotFound)> UpdateProductAsync(int id, ProductCsvDto dto)
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
                .AnyAsync(p => p.SKU == sku && p.Id != id);

            if (skuExists)
            {
                return (null, "This SKU already exists.", false);
            }

            if (dto.Price < 0 || dto.Stock < 0)
            {
                return (null, "Price and stock cannot be negative.", false);
            }

            product.SKU = sku;
            product.Name = dto.Name ?? string.Empty;
            product.Description = dto.Description ?? string.Empty;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Category = dto.Category ?? string.Empty;

            await _context.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            await _logger.LogMessageAsync($"Product updated - ProductId: {id}, SKU: {product.SKU}");
            return (product, null, false);
        }

        // Reads the CSV file and processes the products
        public async Task<(int added, int updated)> UploadCsvAsync(IFormFile file, List<IFormFile>? images)
        {
            await _logger.LogMessageAsync("CSV product upload started.", "Information");
            using var reader = new StreamReader(file.OpenReadStream());

            try
            {

                using var csv = new CsvReader(
                    reader,
                    new CsvConfiguration(CultureInfo.InvariantCulture)// settings to read
                    {
                        HeaderValidated = null,// disables CsvHelper's default behavior of throwing an exception if the CSV headers don't exactly match the DTO's property names
                        MissingFieldFound = null,
                        TrimOptions = TrimOptions.Trim
                    });

                var csvProducts = csv// the rows of the csv file is converted into a list
                    .GetRecords<ProductCsvDto>()
                    .ToList();

                // giving the data to UpsertProductsAsync function
                var (added, updated) = await UpsertProductsAsync(csvProducts, images);
                await _logger.LogMessageAsync($"CSV product upload completed. Added: {added}, Updated: {updated}");
                return (added, updated);
            }
            catch (Exception ex)
            {
                await _logger.LogMessageAsync($"CSV product upload failed: {ex.Message}", "Error");
                throw;
            }
        }

        // csvProducts is the list
        public async Task<(int added, int updated)> UpsertProductsAsync(List<ProductCsvDto> csvProducts, List<IFormFile>? images)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

            var errors = new List<string>();// every problem found is added here

            // dictionary for uploaded images(file name -> file)
            var uploadedFiles = new Dictionary<string, IFormFile>(StringComparer.OrdinalIgnoreCase);

            // images is a list of files(images) that arrived in the upload. nothing is saved to storage now.
            foreach (var f in images ?? new List<IFormFile>())
            {
                if (f.Length == 0)// if image file is empty means there is no image file or image is 0 bytes.
                {
                    continue;
                }

                var uploadedName = Path.GetFileName(f.FileName).Trim();

                // TryAdd returns false if the name already exists in the dictionary not in the database(ToDictionary would crash here, it gives exception if two files have same name when uploading)
                if (!uploadedFiles.TryAdd(uploadedName, f))
                {
                    errors.Add($"Image '{uploadedName}' was uploaded more than once.");
                }
            }

            // checking every CSV row. Nothing is written yet.
            var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);// StringComparer.OrdinalIgnoreCase is the rule for the hashset
            // without the rule ab1 and AB1 will be different sku

            for (int i = 0; i < csvProducts.Count; i++)
            {
                var row = csvProducts[i];
                var line = i + 2;// line 1 of the CSV is the header. this is for the user dispaly. 0+2 shows line 2 means the first data row
                var rowSku = (row.SKU ?? string.Empty).Replace(" ", "").Trim();

                if (rowSku.Length == 0)
                {
                    errors.Add($"Row {line}: SKU is required.");
                }
                else if (!seenSkus.Add(rowSku))// Add returns false if this SKU was already seen. adding to seenSKu happen here
                {
                    errors.Add($"Row {line}: SKU '{rowSku}' appears more than once in the file.");
                }

                if (row.Price < 0 || row.Stock < 0)
                {
                    errors.Add($"Row {line}: price and stock cannot be negative.");
                }

                if (!string.IsNullOrWhiteSpace(row.ImageFiles))
                {// in the current row as per the for loop
                    var names = row.ImageFiles.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    foreach (var imageName in names)
                    {
                        if (!uploadedFiles.TryGetValue(imageName, out var imageFile))
                        {
                            errors.Add($"Row {line}: image '{imageName}' was not uploaded.");
                        }
                        else if (!allowedExtensions.Contains(Path.GetExtension(imageFile.FileName).ToLowerInvariant()))
                        {
                            errors.Add($"Row {line}: image '{imageName}' has an unsupported type.");
                        }
                    }
                }
            }

            // If anything is wrong, stopping here. No files written, nothing saved.
            if (errors.Count > 0)
            {
                throw new Exception(string.Join(Environment.NewLine, errors));
            }

            // loading the products that already exist in the database
            var csvSkus = seenSkus.ToList();

            var existingProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => csvSkus.Contains(p.SKU))
                .ToDictionaryAsync(p => p.SKU, p => p, StringComparer.OrdinalIgnoreCase);// here sku is set as the key and product data is set as the value

            Directory.CreateDirectory(uploadFolder);

            var newFiles = new List<string>();// written now: deleting them if the import fails
            var oldFiles = new List<string>();// replaced: deleting them only if the import succeeds
            int added = 0;
            int updated = 0;

            try
            {
                // creating or updating each product and writing its images
                foreach (var x in csvProducts)
                {
                    var sku = (x.SKU ?? string.Empty).Replace(" ", "").Trim();

                    Product product;// product can store one product object

                    if (existingProducts.TryGetValue(sku, out var existing))
                    {
                        // product exists: update it
                        product = existing;

                        product.SKU = sku;
                        product.Name = x.Name ?? string.Empty;
                        product.Price = x.Price;
                        product.Stock = x.Stock;
                        product.Description = x.Description ?? string.Empty;
                        product.Category = x.Category ?? string.Empty;

                        updated++;

                        if (!string.IsNullOrWhiteSpace(x.ImageFiles))
                        {
                            foreach (var oldImage in existing.Images)
                            {
                                oldFiles.Add(Path.Combine(uploadFolder, Path.GetFileName(oldImage.ImageUrl)));
                            }

                            _context.ProductImages.RemoveRange(existing.Images);// This only marks the image rows for deletion
                        }
                    }
                    else
                    {
                        // product is new, creating it
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
                        existingProducts[sku] = product;
                        added++;
                    }

                    if (!string.IsNullOrWhiteSpace(x.ImageFiles))// x.ImageFiles contains multiple image names in the product row
                    {
                        var imageFileNames = x.ImageFiles.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        foreach (var imageFileName in imageFileNames)
                        {
                            var file = uploadedFiles[imageFileName];
                            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();// file is an IFormFile, shirt.jpg

                            var newFileName = $"{Guid.NewGuid()}{extension}";
                            var filePath = Path.Combine(uploadFolder, newFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {// creating a file in the path and copying the image bytes to the file(image) created in that path
                                await file.CopyToAsync(stream);
                            }

                            newFiles.Add(filePath);

                            product.Images.Add(new ProductImage// Image path is added to ImageUrl field
                            {
                                ImageUrl = $"/images/products/{newFileName}"
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                foreach (var path in newFiles)
                {
                    try { File.Delete(path); } catch { }
                }

                throw;
            }

            foreach (var path in oldFiles)
            {
                try { File.Delete(path); } catch { }
            }

            _cache.Remove(CategoriesCacheKey);

            return (added, updated);
        }

        public async Task<bool> HideProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            product.IsDeleted = true;

            await _context.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            await _logger.LogMessageAsync($"Product hidden - ProductId: {id}, SKU: {product.SKU}");
            return true;
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            product.IsDeleted = false;

            await _context.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);
            await _logger.LogMessageAsync($"Product restored - ProductId: {id}, SKU: {product.SKU}");
            return true;
        }

        public async Task<(bool Success, string? Error, bool NotFound)> PermanentlyDeleteProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return (false, null, true);
            }

            var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);

            if (hasOrders)
            {
                return (false, "This product is part of past orders and cannot be permanently deleted. Hide it instead.", false);
            }

            var sku = product.SKU;

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

            var filePaths = product.Images
                .Select(i => Path.Combine(uploadFolder, Path.GetFileName(i.ImageUrl)))
                .ToList();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            _cache.Remove(CategoriesCacheKey);

            foreach (var path in filePaths)
            {
                try { File.Delete(path); } catch { }
            }

            await _logger.LogMessageAsync($"Product permanently deleted - ProductId: {id}, SKU: {sku}");

            return (true, null, false);
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            if (_cache.TryGetValue(CategoriesCacheKey, out List<string>? cached))
            {
                return cached!;
            }

            var categories = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            _cache.Set(CategoriesCacheKey, categories, TimeSpan.FromMinutes(10));

            return categories;
        }

        public async Task<(bool Success, string? Error, List<ProductImage>? Images)> UploadProductImagesAsync(int id, List<IFormFile> files)
        {
            var productExists = await _context.Products.AnyAsync(p => p.Id == id);

            if (!productExists)
            {
                return (false, "Product not found.", null);
            }

            if (files == null || files.Count == 0)
            {
                return (false, "Please select at least one image.", null);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

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
            }

            Directory.CreateDirectory(uploadFolder);

            var uploadedImages = new List<ProductImage>();
            var writtenFiles = new List<string>();   // undo list

            try
            {
                foreach (var file in files)
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    writtenFiles.Add(filePath);

                    uploadedImages.Add(new ProductImage
                    {
                        ProductId = id,
                        ImageUrl = $"/images/products/{fileName}"
                    });
                }

                if (uploadedImages.Count == 0)
                {
                    return (false, "No valid images were uploaded.", null);
                }

                _context.ProductImages.AddRange(uploadedImages);
                await _context.SaveChangesAsync();
            }
            catch
            {
                foreach (var path in writtenFiles)
                {
                    try { File.Delete(path); } catch { }
                }

                throw;
            }

            await _logger.LogMessageAsync($"Product images uploaded - ProductId: {id}, ImageCount: {uploadedImages.Count}");
            return (true, null, uploadedImages);
        }
    }
}