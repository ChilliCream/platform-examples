using eShop.Catalog.Data.Entities;

namespace eShop.Catalog.Services;

public static class Mappings
{
    public static IQueryable<Product> MapToReadModel(
        this IQueryable<ProductEntity> queryable)
        => queryable.Select(t => new Product
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                TypeId = t.TypeId,
                BrandId = t.BrandId,
                AvailableStock = t.AvailableStock,
                RestockThreshold = t.RestockThreshold,
                MaxStockThreshold = t.MaxStockThreshold,
                OnReorder = t.OnReorder
            });

    public static Product MapToReadModel(
        this ProductEntity entity)
        =>  new Product
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                TypeId = entity.TypeId,
                BrandId = entity.BrandId,
                AvailableStock = entity.AvailableStock,
                RestockThreshold = entity.RestockThreshold,
                MaxStockThreshold = entity.MaxStockThreshold,
                OnReorder = entity.OnReorder
            };

    public static void MapToEntity(this Product readModel, ProductEntity entity)
    {
        entity.Id = readModel.Id;
        entity.Name = readModel.Name;
        entity.Description = readModel.Description;
        entity.Price = readModel.Price;
        entity.TypeId = readModel.TypeId;
        entity.BrandId = readModel.BrandId;
        entity.AvailableStock = readModel.AvailableStock;
        entity.RestockThreshold = readModel.RestockThreshold;
        entity.MaxStockThreshold = readModel.MaxStockThreshold;
        entity.OnReorder = readModel.OnReorder;
    }

    public static IQueryable<Brand> MapToBrand(
        this IQueryable<BrandEntity> queryable)
        => queryable.Select(b => new Brand { Id = b.Id, Name = b.Name });

    public static IQueryable<ProductType> MapToProductType(
        this IQueryable<ProductTypeEntity> queryable)
        => queryable.Select(b => new ProductType { Id = b.Id, Name = b.Name });
}