using ECommerceBackend.Models;
using ECommerceBackend.Models.DTOs;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;// IActionResult

namespace ECommerceBackend.Controllers
{
    [ApiController]// it gives the next line feature
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // ProductController depends on ProductService, and ASP.NET Core's DI container injects a ProductService object into the ProductController constructor.
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
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
                var (added, updated) = await _productService.UploadCsvAsync(file, images);

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
            var result = await _productService.GetProductsAsync(page, pageSize, category);

            return Ok(new
            {
                products = result.Products,
                page,
                pageSize,
                totalProducts = result.TotalProducts,
                totalPages = result.TotalPages
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductAsync(id);

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
            var result = await _productService.GetAllProductsForAdminAsync(page, pageSize);

            return Ok(new
            {
                products = result.Products,
                page,
                pageSize,
                totalProducts = result.TotalProducts,
                totalPages = result.TotalPages
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string search, int page = 1, int pageSize = 20, string category = "")
        {
            var result = await _productService.SearchProductsAsync(
                search,
                page,
                pageSize,
                category);

            return Ok(new
            {
                products = result.Products,
                page,
                pageSize,
                totalProducts = result.TotalProducts,
                totalPages = result.TotalPages
            });
        }

        [HttpGet("admin/search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchProductsForAdmin(string search, int page = 1, int pageSize = 20)
        {
            var result = await _productService.SearchProductsForAdminAsync(search, page,pageSize);

            return Ok(new
            {
                products = result.Products,
                page,
                pageSize,
                totalProducts = result.TotalProducts,
                totalPages = result.TotalPages
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct(ProductCsvDto dto)
        {
            var result = await _productService.AddProductAsync(dto);

            if (result.Product == null)
            {
                return BadRequest(new
                {
                    message = result.Error
                });
            }

            return CreatedAtAction(nameof(GetProduct), new { id = result.Product.Id }, result.Product);
        }
        // nameof() is a C# keyword that converts a method name into a string at compile-time.
        // nameof(GetProduct) becomes "GetProduct".
        // this return means it says the ASP.NET Core to find the action method called GetProduct in this controller and find its route and use it to the build the url

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCsvDto dto)
        {
            var result = await _productService.UpdateProductAsync(id, dto);

            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.Product == null)
            {
                return BadRequest(new
                {
                    message = result.Error
                });
            }

            return Ok(result.Product);
        }

        [HttpPut("{id}/hide")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HideProduct(int id)
        {
            var success = await _productService.HideProductAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                message = "Product hidden successfully"
            });
        }

        [HttpPut("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var success = await _productService.RestoreProductAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                message = "Product made visible successfully"
            });
        }

        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PermanentlyDeleteProduct(int id)
        {
            var success = await _productService.PermanentlyDeleteProductAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                message = "Product permanently deleted"
            });
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();

            return Ok(categories);
        }

        // runs when individual Add Product / Update Product form
        [HttpPost("{id}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadProductImages(int id, List<IFormFile> files)
        {
            var result = await _productService.UploadProductImagesAsync(id, files);

            if (!result.Success)
            {
                if (result.Error == "Product not found.")
                {
                    return NotFound(new
                    {
                        message = result.Error
                    });
                }

                return BadRequest(new
                {
                    message = result.Error
                });
            }

            return Ok(new
            {
                message = "Product images uploaded successfully.",
                images = result.Images!.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.ImageUrl
                })
            });
        }
    }
}