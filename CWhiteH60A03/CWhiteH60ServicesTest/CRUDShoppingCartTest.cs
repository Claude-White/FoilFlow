using CWhiteH60Services.DAL;
using CWhiteH60Services.Models;
using Microsoft.EntityFrameworkCore;

namespace CWhiteH60ServicesTest;

public class CRUDShoppingCartTest : IDisposable {
    
    private readonly H60AssignmentDbCWContext _context;
    private readonly IShoppingCartRepository<ShoppingCart> _shoppingCartRepository;

    public CRUDShoppingCartTest() {
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
            new CartItem { CartId = 1, ProductId = 19, Quantity = 1 },
            new CartItem { CartId = 1, ProductId = 7, Quantity = 3 },
            new CartItem { CartId = 1, ProductId = 14, Quantity = 1 },
            new CartItem { CartId = 2, ProductId = 2, Quantity = 2 }
        );
        
        _context.SaveChanges();

        _shoppingCartRepository = new ShoppingCartRepository(_context);
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
    public async void Test_Create_ShoppingCart() {
        // Arrange
        var shoppingCart = new ShoppingCart {
            CartId = 3, 
            CustomerId = 3, 
            DateCreated = DateTime.Now
        };
        // Act
        Assert.True(await _shoppingCartRepository.Create(shoppingCart));
        // Assert
        Assert.Equal(3, _context.ShoppingCarts.ToList().Count);
        var customerShoppingCart = _context
            .Customers
            .Include(c => c.ShoppingCart)
            .First(c => c.CustomerId == 3)
            .ShoppingCart;
        Assert.NotNull(customerShoppingCart);
    }
    
    [Fact]
    public async void Test_Read_ShoppingCart() {
        // Arrange
        // In Constructor
        // Act
        var shoppingCarts = await _shoppingCartRepository.Read();
        // Assert
        Assert.Equal(2, shoppingCarts.Count);
        Assert.Equal(3, shoppingCarts.First(sc => sc.CartId == 1).CartItems.Count);
        Assert.Equal(1, shoppingCarts.First(sc => sc.CartId == 2).CartItems.Count);
    }
    
    [Fact]
    public async void Test_Update_ShoppingCart() {
        // Arrange
        var shoppingCart = _context
            .ShoppingCarts
            .Include(sc => sc.CartItems)
            .First(sc => sc.CartId == 2);
        // Assert
        Assert.NotNull(shoppingCart);
        Assert.Equal(1, shoppingCart.CartItems.Count);
        // Act
        shoppingCart
            .CartItems
            .Add(new CartItem {
                CartId = 2, 
                ProductId = 1, 
                Quantity = 1
            });
        Assert.True(await _shoppingCartRepository.Update(shoppingCart));
        // Assert
        Assert.Equal(2, _context
            .ShoppingCarts
            .Include(sc => sc.CartItems)
            .First(sc => sc.CartId == 2)
            .CartItems
            .Count);
        Assert.Equal(2, _context
            .ShoppingCarts
            .First(sc => sc.CartId == 2)
            .CustomerId);
    }

    [Fact]
    public async void Test_Delete_ShoppingCart() {
        // Arrange
        var shoppingCart = _context
            .ShoppingCarts
            .First(sc => sc.CartId == 2);

        const int emptyCartId = 4;
        _context.ShoppingCarts.Add(new ShoppingCart {
            CartId = emptyCartId, 
            CustomerId = 4, 
            DateCreated = DateTime.Now
        });
        await _context.SaveChangesAsync();
        var emptyShoppingCart = _context
            .ShoppingCarts
            .First(sc => sc.CartId == emptyCartId);
        // Assert
        Assert.NotNull(shoppingCart);
        Assert.NotNull(emptyShoppingCart);
        // Act
        Assert.False(await _shoppingCartRepository.Delete(shoppingCart));
        Assert.True(await _shoppingCartRepository.Delete(emptyShoppingCart));
        // Assert
        Assert.NotNull(_context
            .ShoppingCarts
            .First(sc => sc.CartId == 2));
        Assert.Null(_context
            .ShoppingCarts
            .FirstOrDefault(sc => sc.CartId == emptyCartId));
    }
    
    [Fact]
    public async void Test_Find_ShoppingCart() {
        // Arrange
        // In Constructor
        // Act
        var foundShoppingCart = await _shoppingCartRepository.Find(1);
        // Assert
        Assert.NotNull(foundShoppingCart);
        Assert.Equal(1, _context
            .ShoppingCarts
            .First(sc => sc.CartId == 1)
            .CustomerId);
        Assert.Equal(3, _context
            .ShoppingCarts
            .Include(sc => sc.CartItems)
            .First(sc => sc.CartId == 1)
            .CartItems
            .Count);
    }
}