using Bookshop.Domain.Authors;

namespace Bookshop.Domain.Tests.Authors;

public class AuthorTests
{
    [Fact]
    public void Create_WithValidNameAndBio_SetsProperties()
    {
        var author = Author.Create("Eric Evans", "Author of Domain-Driven Design.");

        Assert.Equal("Eric Evans", author.Name);
        Assert.Equal("Author of Domain-Driven Design.", author.Bio);
    }

    [Fact]
    public void Create_WithoutBio_LeavesBioNull()
    {
        var author = Author.Create("Eric Evans");

        Assert.Null(author.Bio);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Author.Create(name));
    }
}
