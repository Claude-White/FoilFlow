using CWhiteH60Services.DAL;
using CWhiteH60Services.Models;
using Microsoft.EntityFrameworkCore;

namespace CWhiteH60ServicesTest;

public class CRUDCartItemTest : IDisposable {
    private readonly H60AssignmentDbCWContext _context;
    private readonly ICartItemRepository<CartItem> _cartItemRepository;

    public CRUDCartItemTest() {
        var options = new DbContextOptionsBuilder<H60AssignmentDbCWContext>()
            .UseInMemoryDatabase(databaseName: new Guid().ToString())
            .Options;

        _context = new H60AssignmentDbCWContext(options);
        _context.Database.EnsureCreated();
        
        _context.ProductCategories.AddRange(
            new ProductCategory { CategoryId = 1, ProdCat = "Foil Boards" },
            new ProductCategory { CategoryId = 2, ProdCat = "Front Wings" },
            new ProductCategory { CategoryId = 3, ProdCat = "Stabilizers" },
            new ProductCategory { CategoryId = 4, ProdCat = "Fuselage" },
            new ProductCategory { CategoryId = 5, ProdCat = "Masts" }
        );
        
        _context.Products.AddRange(
            new Product { 
                ProductId = 1, 
                ProdCatId = 2, 
                Description = "Sonar MA1850v2", 
                Manufacturer = "North", 
                Stock = 4, 
                BuyPrice = 750.00m, 
                SellPrice = 900.00m },
            new Product { 
                ProductId = 2, 
                ProdCatId = 2, 
                Description = "G 1000 Front Wing V1", 
                Manufacturer = "Slingshot", 
                Stock = 8, 
                BuyPrice = 550.00m, 
                SellPrice = 630.00m },
            new Product {
                ProductId = 7, 
                ProdCatId = 4, 
                Description = "Black Standard Fuselage", 
                Manufacturer = "Axis", 
                Stock = 34, 
                BuyPrice = 300.00m, 
                SellPrice = 350.00m
            },
            new Product {
                ProductId = 14, 
                ProdCatId = 3, 
                Description = "Phantasm Stabilizer 340 Turbo-Tail V1", 
                Manufacturer = "Slingshot", 
                Stock = 46, 
                BuyPrice = 195.00m, 
                SellPrice = 230.00m
            },
            new Product {
                ProductId = 19, 
                ProdCatId = 5, 
                Description = "CARBON MAST 16", 
                Manufacturer = "F-ONE", 
                Stock = 5, 
                BuyPrice = 380.00m, 
                SellPrice = 470.00m
            }
        );

        _context.Customers.AddRange(
            new Customer {
                CustomerId = 1, 
                FirstName = "Claude", 
                LastName = "White", 
                Email = "claude@white.com",
                PhoneNumber = "1231231234", 
                Province = "QC", 
                CreditCard = "1234123412341234",
                UserId = ""
            },
            new Customer {
                CustomerId = 2, 
                FirstName = "Ryan", 
                LastName = "Somers", 
                Email = "ryan@somers.com",
                PhoneNumber = "1231231234", 
                Province = "QC", 
                CreditCard = "1234123412341234",
                UserId = ""
            },
            new Customer {
                CustomerId = 3, 
                FirstName = "Pierre", 
                LastName = "Badra", 
                Email = "pierre@badra.com",
                PhoneNumber = "1231231234", 
                Province = "QC", 
                CreditCard = "1234123412341234",
                UserId = ""
            },
            new Customer {
                CustomerId = 4, 
                FirstName = "Matteo", 
                LastName = "Falasconi", 
                Email = "matteo@falasconi.com",
                PhoneNumber = "1231231234", 
                Province = "QC", 
                CreditCard = "1234123412341234",
                UserId = ""
            }
        );
        
        _context.ShoppingCarts.AddRange(
            new ShoppingCart {
                CartId = 1, 
                CustomerId = 1, 
                DateCreated = DateTime.Now
            },
            new ShoppingCart {
                CartId = 2, 
                CustomerId = 2, 
                DateCreated = DateTime.Now
            }
        );
        
        _context.CartItems.AddRange(
            new CartItem {
                CartItemId = 1, 
                CartId = 1, 
                ProductId = 19, 
                Quantity = 1
            },
            new CartItem {
                CartItemId = 2, 
                CartId = 1, 
                ProductId = 7, 
                Quantity = 3
            },
            new CartItem 
            {
                CartItemId = 3, 
                CartId = 1, 
                ProductId = 14, 
                Quantity = 1
            },
            new CartItem 
            {
                CartItemId = 4, 
                CartId = 2, 
                ProductId = 2,
                Quantity = 2
            }
        );
        
        _context.SaveChanges();

        _cartItemRepository = new CartItemRepository(_context);
    }

    public void Dispose()
    {
        _context.CartItems.RemoveRange(_context.CartItems);
        _context.ShoppingCarts.RemoveRange(_context.ShoppingCarts);
        _context.Customers.RemoveRange(_context.Customers);
        _context.Products.RemoveRange(_context.Products);
        _context.ProductCategories.RemoveRange(_context.ProductCategories);
        
        _context.SaveChanges();
        
        _context.Dispose();
    }
    
    [Fact]
    public async void Test_Create_CartItem() {
        // Arrange
        var cartItem = new CartItem {
            CartItemId = 5,
            CartId = 2,
            ProductId = 2,
            Quantity = 3
        };
        // Act
        Assert.True(await _cartItemRepository.Create(cartItem));
        // Assert
        var customerShoppingCart = _context
            .Customers
            .Include(c => c.ShoppingCart)
            .ThenInclude(shoppingCart => shoppingCart!.CartItems)
            .First(c => c.ShoppingCart!.CartId == 2)
            .ShoppingCart;
        Assert.NotNull(customerShoppingCart!
            .CartItems
            .FirstOrDefault(ci => ci.CartItemId == 5));
    }
    
    [Fact]
    public async void Test_Read_CartItem() {
        // Arrange
        // In Constructor
        // Act
        var cartItems = await _cartItemRepository.Read();
        // Assert
        Assert.Equal(4, cartItems.Count);
        Assert.Equal(1, cartItems
            .First(sc => sc.CartItemId == 1)
            .Cart.CartId);
        Assert.Equal(2, cartItems
            .First(sc => sc.CartItemId == 4)
            .Cart.CartId);
    }
    
    [Fact]
    public async void Test_Update_CartItem() {
        // Arrange
        var cartItem = _context
            .CartItems
            .Include(ci => ci.Cart)
            .First(ci => ci.CartItemId == 1);
        // Assert
        Assert.NotNull(cartItem);
        Assert.Equal(1, cartItem.Cart.CartId);
        // Act
        cartItem.Quantity = 7;
        Assert.True(await _cartItemRepository.Update(cartItem));
        // Assert
        Assert.Equal(7, _context
            .CartItems
            .First(ci => ci.CartItemId == 1)
            .Quantity);
    }

    [Fact]
    public async void Test_Delete_CartItem() {
        // Arrange
        var cartItem = _context
            .CartItems
            .Include(ci => ci.Cart)
            .First(ci => ci.CartItemId == 1);
        var shoppingCart = cartItem.Cart;
        // Assert
        Assert.NotNull(cartItem);
        Assert.NotNull(shoppingCart);
        // Act
        Assert.True(await _cartItemRepository.Delete(cartItem));
        // Assert
        Assert.Null(_context
            .CartItems
            .FirstOrDefault(ci => ci.CartItemId == 1));
        Assert.NotNull(_context
            .ShoppingCarts
            .FirstOrDefault(sc => sc.CartId == 1));
    }
    
    [Fact]
    public async void Test_Find_CartItem() {
        // Arrange
        // In Constructor
        // Act
        var foundCartItem = await _cartItemRepository.Find(1);
        // Assert
        Assert.NotNull(foundCartItem);
        Assert.Equal(1, _context
            .CartItems
            .First(ci => ci.CartItemId == 1)
            .Quantity);
        Assert.Equal(19, _context
            .CartItems
            .First(ci => ci.CartItemId == 1)
            .ProductId);
    }
}