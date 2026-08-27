using System.Linq.Expressions;
using Bookshop.Application.Authors.ReadModels;
using Bookshop.Application.Books.ReadModels;
using Bookshop.Application.Customers.ReadModels;
using Bookshop.Application.Orders.ReadModels;
using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Bookshop.Domain.Customers;
using Bookshop.Domain.Orders;

namespace Bookshop.Infrastructure.Common.Database;

/// <summary>
/// Shared <c>AsNoTracking</c> projections from the EF-mapped aggregates into the Application
/// read models. Used by the source-generated DataLoaders in
/// Infrastructure/&lt;Feature&gt;/DataLoaders, so each read-model shape is defined exactly once
/// next to <see cref="BookshopContext"/>.
/// </summary>
internal static class ReadModelProjections
{
    public static Expression<Func<Author, AuthorReadModel>> ToAuthorReadModel { get; } =
        author => new AuthorReadModel
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
        };

    public static Expression<Func<Book, BookReadModel>> ToBookReadModel { get; } =
        book => new BookReadModel
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn,
            Price = book.Price,
            Currency = book.Currency,
            AuthorId = book.AuthorId,
            StockQuantity = book.StockQuantity,
        };

    public static Expression<Func<Customer, CustomerReadModel>> ToCustomerReadModel { get; } =
        customer => new CustomerReadModel
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
        };

    public static Expression<Func<Order, OrderReadModel>> ToOrderReadModel { get; } =
        order => new OrderReadModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            TotalAmount = order.Lines.Sum(line => line.UnitPrice * line.Quantity),
            TotalCurrency = order.Lines.Select(line => line.Currency).First(),
            Lines = order.Lines
                .Select(line => new OrderLineReadModel
                {
                    Id = line.Id,
                    BookId = line.BookId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    Currency = line.Currency,
                })
                .ToList(),
        };
}
