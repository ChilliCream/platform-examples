using Bookshop.Domain.Common;

namespace Bookshop.Domain.Customers;

/// <summary>
/// A customer who can place orders in the shop.
/// </summary>
public sealed class Customer : Entity
{
    public string Name { get; private set; }

    public string Email { get; private set; }

    private Customer(Guid id, string name, string email)
        : base(id)
    {
        Name = name;
        Email = email;
    }

    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Customer email must be a valid email address.", nameof(email));
        }

        return new Customer(Guid.NewGuid(), name.Trim(), email.Trim());
    }
}
