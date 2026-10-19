namespace eShop.Catalog.Services.Errors;

public class ProductPriceCannotBeNegative : Exception
{
    public ProductPriceCannotBeNegative(decimal price)
        : base($"The product price cannot be negative. Current price: {price}")
    {
        Price = price;
    }
    
    public decimal Price { get; }
}