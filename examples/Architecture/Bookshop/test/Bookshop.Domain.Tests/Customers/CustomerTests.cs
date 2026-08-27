using Bookshop.Domain.Customers;

namespace Bookshop.Domain.Tests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidNameAndEmail_SetsProperties()
    {
        var customer = Customer.Create("Ada Lovelace", "ada@example.com");

        Assert.Equal("Ada Lovelace", customer.Name);
        Assert.Equal("ada@example.com", customer.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(name, "ada@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        Assert.Throws<ArgumentException>(() => Customer.Create("Ada Lovelace", email));
    }
}
