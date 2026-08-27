using Bookshop.Domain.Authors;
using Bookshop.Domain.Books;
using Bookshop.Domain.Customers;
using Bookshop.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Bookshop.Infrastructure.Common.Database;

public sealed class BookshopContext(DbContextOptions<BookshopContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(builder =>
        {
            builder.ToTable("authors");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Ignore(x => x.DomainEvents);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Bio).HasMaxLength(2000);
        });

        modelBuilder.Entity<Book>(builder =>
        {
            builder.ToTable("books");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Ignore(x => x.DomainEvents);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Isbn).HasMaxLength(13).IsRequired();
            builder.HasIndex(x => x.Isbn).IsUnique();
            builder.Property(x => x.AuthorId);
            builder.Property(x => x.StockQuantity).IsRequired();
            builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable("customers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Ignore(x => x.DomainEvents);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(320);
        });

        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.CustomerId);
            builder.Property(x => x.Status).IsRequired();

            // The Total is computed from Lines and the domain events collection is an
            // in-memory-only notification buffer; neither is persisted.
            builder.Ignore(x => x.Total);
            builder.Ignore(x => x.Currency);
            builder.Ignore(x => x.DomainEvents);

            var linesBuilder = builder.OwnsMany(x => x.Lines);
            linesBuilder.ToTable("order_lines");
            linesBuilder.WithOwner().HasForeignKey("order_id");
            linesBuilder.HasKey(x => x.Id);
            linesBuilder.Property(x => x.Id).ValueGeneratedNever();
            linesBuilder.Ignore(x => x.DomainEvents);
            linesBuilder.Property(x => x.BookId);
            linesBuilder.Property(x => x.Quantity).IsRequired();
            linesBuilder.Ignore(x => x.LineTotal);
            linesBuilder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
            linesBuilder.Property(x => x.Currency).HasMaxLength(3).IsRequired();

            builder.Navigation(x => x.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

}
