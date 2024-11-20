using System;
using System.Collections.Generic;

namespace CWhiteH60Customer.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [UniqueEmailAnnotation]
    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Province { get; set; }

    public string? CreditCard { get; set; }

    public string UserId { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ShoppingCart? ShoppingCart { get; set; }
}
