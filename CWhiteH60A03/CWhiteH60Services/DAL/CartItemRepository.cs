using CWhiteH60Services.Models;
using Microsoft.EntityFrameworkCore;

namespace CWhiteH60Services.DAL;

public class CartItemRepository : ICartItemRepository<CartItem> {
    private readonly H60AssignmentDbCWContext _context;

    public CartItemRepository(H60AssignmentDbCWContext context) {
        _context = context;
    }
    
    public async Task<bool> Create(CartItem cartItem) {
        var product = await _context.Products.FindAsync(cartItem.ProductId);
        if (product != null && cartItem.Quantity != null) {
            product.Stock -= cartItem.Quantity.Value;
            if (product.Stock < 0) {
                return false;
            }
            _context.Products.Update(product);
        }
        await _context.CartItems.AddAsync(cartItem);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<List<CartItem>> Read() {
        return await _context
            .CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .ToListAsync();
    }

    public async Task<bool> Update(CartItem cartItem) {
        _context.CartItems.Update(cartItem);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> Delete(CartItem cartItem) {
        var product = await _context.Products.FindAsync(cartItem.ProductId);
        if (product != null && cartItem.Quantity != null) {
            product.Stock += cartItem.Quantity.Value;
            _context.Products.Update(product);
        }
        _context.CartItems.Remove(cartItem);
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }

    public async Task<CartItem> Find(int id) {
        return await _context
            .CartItems
            .FirstOrDefaultAsync(ci => ci.CartItemId == id);
    }
}