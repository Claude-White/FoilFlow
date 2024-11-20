using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.EntityFrameworkCore;
using CWhiteH60Services.DAL;
using CWhiteH60Services.Models;
using CWhiteH60Services.Models.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace CWhiteH60Services.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IStoreRepository<Product> _storeRepository;

    public ProductsController(IStoreRepository<Product> storeRepo)
    {
        _storeRepository = storeRepo;
    }
    
    [HttpGet]
    public ActionResult<List<Product>> AllProducts([FromQuery] string? productName)
    {
        if (!string.IsNullOrEmpty(productName)) {
            return _storeRepository.GetAll().Where(p => p.Description.Contains(productName, StringComparison.InvariantCultureIgnoreCase) || p.ProdCat.ProdCat.Contains(productName, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
        return _storeRepository.GetAll().ToList();
    }

    [HttpGet("/api/ProductsByCategory/{id:int}")]
    public ActionResult<List<Product>> FilteredProducts(int id) {
        if (id == 0) return _storeRepository.GetAll();
        return _storeRepository.GetAll().Where(p => p.ProdCat.CategoryId == id).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> Details(int id)
    {
        var product = await _storeRepository.GetByIdInclude(id);
        if (product == null)
        {
            return NotFound();
        }
        
        return product;
    }
    
    [HttpGet("/api/CustomerProducts")]
    public ActionResult<List<CustomerProductDto>> AllProductsCustomer([FromQuery] string? productName)
    {
        if (!string.IsNullOrEmpty(productName)) {
            var test = _storeRepository.GetAll()
                .Where(p => p.Description.Contains(productName, StringComparison.InvariantCultureIgnoreCase) ||
                            p.ProdCat.ProdCat.Contains(productName, StringComparison.InvariantCultureIgnoreCase))
                .Select(p => new CustomerProductDto(p)).ToList();
            return test;
        }
        return _storeRepository.GetAll().Select(p => new CustomerProductDto(p)).ToList();
    }

    [HttpGet("/api/CustomerProductsByCategory/{id:int}")]
    public ActionResult<List<CustomerProductDto>> FilteredProductsCustomer(int id) {
        if (id == 0) return _storeRepository.GetAll().Select(p => new CustomerProductDto(p)).ToList();
        return _storeRepository.GetAll().Where(p => p.ProdCat.CategoryId == id).Select(p => new CustomerProductDto(p)).ToList();
    }

    [HttpGet("/api/CustomerProducts/{id:int}")]
    public async Task<ActionResult<CustomerProductDto>> DetailsCustomer(int id)
    {
        var product = await _storeRepository.GetByIdInclude(id);
        if (product == null)
        {
            return NotFound();
        }
        
        var customerProduct = new CustomerProductDto(product);
        
        return customerProduct;
    }
    
    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductDto productDto) {
        var product = new Product() {
            ProductId = productDto.ProductId,
            ProdCatId = productDto.ProdCatId,
            Description = productDto.Description,
            Manufacturer = productDto.Manufacturer,
            Stock = productDto.Stock,
            BuyPrice = productDto.BuyPrice,
            SellPrice = productDto.SellPrice,
            Notes = productDto.Notes,
            ImageName = productDto.ImageName,
            ImageData = productDto.ImageData
        };

        if (ModelState.IsValid)
        {
            await _storeRepository.Create(product);
            await _storeRepository.Save();
            return CreatedAtAction(nameof(Create), new { id = product.ProductId }, product);
        }
        
        return BadRequest(ModelState);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Edit(int id, ProductDto productDto) {
        var product = new Product() {
            ProductId = productDto.ProductId,
            ProdCatId = productDto.ProdCatId,
            Description = productDto.Description,
            Manufacturer = productDto.Manufacturer,
            BuyPrice = productDto.BuyPrice,
            SellPrice = productDto.SellPrice,
            Stock = productDto.Stock,
            Notes = productDto.Notes,
            ImageName = productDto.ImageName,
            ImageData = productDto.ImageData
        };
        
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _storeRepository.Update(product);
                await _storeRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return Ok(product);
        }
        
        return BadRequest(ModelState);
    }

    [HttpPatch("Stock/{id:int}")]
    public async Task<ActionResult<Product>> EditStock(int id, ProductStockDto productStockDto)
    {
        var product = await _storeRepository.GetById(id);
        
        if (product.Stock + productStockDto.Stock < 0)
        {
            ModelState.AddModelError("Stock", "Stock must be greater than 0");
        }

        if (ModelState.IsValid)
        {
            product.Stock += productStockDto.Stock;
            try
            {
                await _storeRepository.Update(product);
                await _storeRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return Ok(product);
        }
        
        return BadRequest(ModelState);
    }

    [HttpPatch("Price/{id:int}")]
    public async Task<ActionResult<Product>> EditPrice(int id, ProductPriceDto productPriceDto)
    {
        if (id != productPriceDto.ProductId)
        {
            return NotFound();
        }
        
        var product = await _storeRepository.GetById(id);
        
        product.BuyPrice = productPriceDto.BuyPrice;
        product.SellPrice = productPriceDto.SellPrice;

        if (ModelState.IsValid)
        {
            try
            {
                await _storeRepository.Update(product);
                await _storeRepository.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return Ok(product);
        }

        return BadRequest(ModelState);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var product = await _storeRepository.GetByIdInclude(id);

        if (product != null)
        {
            await _storeRepository.Delete(product);
            await _storeRepository.Save();
            return NoContent();
        }
        
        return NotFound();
    }
    
    [NonAction]
    private bool Exists(int id)
    {
        return _storeRepository.GetAll().Any(e => e.ProductId == id);
    }
}