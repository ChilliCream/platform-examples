using Bookshop.Domain.Common;

namespace Bookshop.Domain.Books;

/// <summary>
/// A book offered for sale, tracked with its own price and stock level.
/// </summary>
public sealed class Book : Entity
{
    public string Title { get; private set; }

    public string Isbn { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; }

    public Guid AuthorId { get; private set; }

    public int StockQuantity { get; private set; }

    private Book(
        Guid id,
        string title,
        string isbn,
        decimal price,
        string currency,
        Guid authorId,
        int stockQuantity)
        : base(id)
    {
        Title = title;
        Isbn = isbn;
        Price = price;
        Currency = currency;
        AuthorId = authorId;
        StockQuantity = stockQuantity;
    }

    public static Book Create(
        string title,
        string isbn,
        decimal price,
        string currency,
        Guid authorId,
        int initialStock = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidBookException("Book title must not be empty.");
        }

        if (initialStock < 0)
        {
            throw new InvalidBookException("Initial stock cannot be negative.");
        }

        if (price < 0)
        {
            throw new InvalidBookException("Book price cannot be negative.");
        }

        return new Book(
            Guid.NewGuid(),
            title.Trim(),
            NormalizeIsbn(isbn),
            price,
            NormalizeCurrency(currency),
            authorId,
            initialStock);
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Restock quantity must be positive.");
        }

        StockQuantity += quantity;
    }

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Removed quantity must be positive.");
        }

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException(
                $"Cannot remove {quantity} unit(s); only {StockQuantity} in stock.");
        }

        StockQuantity -= quantity;
    }

    public void ChangePrice(decimal price, string currency)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "Book price cannot be negative.");
        }

        Price = price;
        Currency = NormalizeCurrency(currency);
    }

    public static string NormalizeIsbn(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidBookException("ISBN must not be empty.");
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (digits.Length != 13)
        {
            throw new InvalidBookException("ISBN-13 must contain exactly 13 digits.");
        }

        var checksum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i] - '0';
            checksum += i % 2 == 0 ? digit : digit * 3;
        }

        if (checksum % 10 != 0)
        {
            throw new InvalidBookException("ISBN-13 checksum is invalid.");
        }

        return digits;
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new InvalidBookException("Currency must not be empty.");
        }

        return currency.Trim().ToUpperInvariant();
    }
}
