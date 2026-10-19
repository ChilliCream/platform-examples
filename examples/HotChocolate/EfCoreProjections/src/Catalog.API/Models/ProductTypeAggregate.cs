namespace eShop.Catalog.Services;

public class ProductTypeAggregate
{
    public required string Type { get; init; }
    public decimal AveragePrice { get; init; }
    public decimal MaxPrice { get; init; }
    public decimal MinPrice { get; init; }
    public int Count { get; init; }
}