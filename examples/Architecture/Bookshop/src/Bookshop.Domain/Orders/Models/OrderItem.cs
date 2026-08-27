using Bookshop.Domain.Books;

namespace Bookshop.Domain.Orders;

public sealed record OrderItem(Book Book, int Quantity);
