using CWhiteH60Customer.DAL;
using CWhiteH60Customer.Models;
using Microsoft.AspNetCore.Mvc;

namespace CWhiteH60Customer.Controllers;

public class ShoppingCartController : Controller {
    private readonly IShoppingCartRepository<ShoppingCart> _shoppingCartRepository;
    private readonly ICartItemRepository<CartItem> _cartItemRepository;
    private readonly IProductRepository<Product> _productRepository;

    public ShoppingCartController(IShoppingCartRepository<ShoppingCart> shoppingCartRepository, ICartItemRepository<CartItem> cartItemRepository, IProductRepository<Product> productRepository) {
        _shoppingCartRepository = shoppingCartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }
    
    [HttpGet]
    public async Task<ActionResult> ShoppingCart(int id) {
        var shoppingCart = await _shoppingCartRepository.GetByCustomerIdInclude(id);
        return View(shoppingCart);
    }

    [HttpPost("AddToCart")]
    public async Task<ActionResult> AddToCart([FromForm] int customerId, int productId, int quantity) {
        var product = await _productRepository.GetById(productId);
        var cart = await _shoppingCartRepository.GetByCustomerId(customerId);
        var cartItem = new CartItem {
            CartId = cart.CartId,
            ProductId = product.ProductId,
            Quantity = quantity,
            Price = product.SellPrice
        };
        var success = await _cartItemRepository.Create(cartItem);
        if (!success) {
            return BadRequest();
        }
        return Ok();
    }

    [HttpGet("RemoveFromCart/{id:int}")]
    public async Task<ActionResult> RemoveFromCart(int id) {
        var cartItem = await _cartItemRepository.GetById(id);
        var success = await _cartItemRepository.Delete(cartItem);
        if (!success) {
            return BadRequest();
        }
        return Ok();
    }
}