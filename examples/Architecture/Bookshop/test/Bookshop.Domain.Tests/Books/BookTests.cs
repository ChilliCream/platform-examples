using Bookshop.Domain.Books;

namespace Bookshop.Domain.Tests.Books;

public class BookTests
{
    private static Book CreateBook(int initialStock = 5) =>
        Book.Create(
            "Domain-Driven Design",
            "9780306406157",
            29.99m,
            "USD",
            Guid.NewGuid(),
            initialStock);

    [Fact]
    public void Create_WithBlankTitle_Throws()
    {
        Assert.Throws<InvalidBookException>(() =>
            Book.Create(" ", "9780306406157", 10m, "USD", Guid.NewGuid()));
    }

    [Fact]
    public void Create_WithNegativeInitialStock_Throws()
    {
        Assert.Throws<InvalidBookException>(() =>
            Book.Create("Title", "9780306406157", 10m, "USD", Guid.NewGuid(), -1));
    }

    [Fact]
    public void Create_NormalizesIsbnAndCurrency()
    {
        var book = Book.Create(
            "Title", "978-0-306-40615-7", 10m, "usd", Guid.NewGuid());

        Assert.Equal("9780306406157", book.Isbn);
        Assert.Equal("USD", book.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-isbn")]
    [InlineData("9780306406158")]
    public void Create_WithInvalidIsbn_Throws(string isbn)
    {
        Assert.Throws<InvalidBookException>(() =>
            Book.Create("Title", isbn, 10m, "USD", Guid.NewGuid()));
    }
    [Fact]
    public void Restock_WithPositiveQuantity_IncreasesStock()
    {
        var book = CreateBook(initialStock: 5);

        book.Restock(3);

        Assert.Equal(8, book.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Restock_WithNonPositiveQuantity_Throws(int quantity)
    {
        var book = CreateBook();

        Assert.Throws<ArgumentOutOfRangeException>(() => book.Restock(quantity));
    }

    [Fact]
    public void RemoveStock_WithinAvailableStock_DecreasesStock()
    {
        var book = CreateBook(initialStock: 5);

        book.RemoveStock(2);

        Assert.Equal(3, book.StockQuantity);
    }

    [Fact]
    public void RemoveStock_MoreThanAvailable_Throws()
    {
        var book = CreateBook(initialStock: 2);

        Assert.Throws<InvalidOperationException>(() => book.RemoveStock(3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RemoveStock_WithNonPositiveQuantity_Throws(int quantity)
    {
        var book = CreateBook();

        Assert.Throws<ArgumentOutOfRangeException>(() => book.RemoveStock(quantity));
    }

    [Fact]
    public void ChangePrice_UpdatesPrice()
    {
        var book = CreateBook();
        book.ChangePrice(24.99m, "eur");

        Assert.Equal(24.99m, book.Price);
        Assert.Equal("EUR", book.Currency);
    }
}
