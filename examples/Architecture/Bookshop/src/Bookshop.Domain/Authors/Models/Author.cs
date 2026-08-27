using Bookshop.Domain.Common;

namespace Bookshop.Domain.Authors;

/// <summary>
/// An author whose books are sold in the shop.
/// </summary>
public sealed class Author : Entity
{
    public string Name { get; private set; }

    public string? Bio { get; private set; }

    private Author(Guid id, string name, string? bio)
        : base(id)
    {
        Name = name;
        Bio = bio;
    }

    public static Author Create(string name, string? bio = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name must not be empty.", nameof(name));
        }

        return new Author(Guid.NewGuid(), name.Trim(), bio);
    }
}
