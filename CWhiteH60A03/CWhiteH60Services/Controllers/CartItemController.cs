using CWhiteH60Services.DAL;
using CWhiteH60Services.Models;
using CWhiteH60Services.Models.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace CWhiteH60Services.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartItemController : ControllerBase {
    private readonly ICartItemRepository<CartItem> _cartItemRepository;

    public CartItemController(ICartItemRepository<CartItem> cartItemRepository) {
        _cartItemRepository = cartItemRepository;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<CartItem>>> AllCartItems() {
        return await _cartItemRepository.Read();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CartItem>> FindCartItem(int id) {
        var cartItem = await _cartItemRepository.Find(id);
        if (cartItem == null) {
            return NotFound();
        }
        return cartItem;
    }

    [HttpPost]
    public async Task<ActionResult> CreateCart(CartItemDto cartItemDto) {
        var cartItem = new CartItem(cartItemDto);
        var success = await _cartItemRepository.Create(cartItem);
        if (!success) {
            return BadRequest(ModelState);
        }
        return CreatedAtAction(nameof(CreateCart), new { id = cartItem.CartItemId }, cartItem);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCart(CartItemDto cartItemDto) {
        var cartItem = new CartItem(cartItemDto);
        var success = await _cartItemRepository.Update(cartItem);
        if (!success) {
            return BadRequest(ModelState);
        }
        return Ok(cartItem);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCart(int id) {
        var cartItem = await _cartItemRepository.Find(id);
        if (cartItem == null) {
            return NotFound();
        }
        
        var success = await _cartItemRepository.Delete(cartItem);
        if (!success) {
            return BadRequest(ModelState);
        }
        return NoContent();
    }
}