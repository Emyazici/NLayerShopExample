using System.Linq.Expressions;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Mapping;

public static class ProductMappings
{
    // EF Core translates the column access to SQL; Convert.ToBase64String runs on the materialized row.
    public static readonly Expression<Func<Product, ProductDetailDto>> ToDetailDto = p => new ProductDetailDto(
        p.Id,
        p.Name,
        p.Price,
        p.Description,
        p.StockCount,
        p.CategoryId,
        p.Category != null ? p.Category.Name : null,
        Convert.ToBase64String(p.RowVersion!));

    public static Product ToEntity(this ProductCreateDto dto) => new()
    {
        Name = dto.Name,
        Price = dto.Price,
        Description = dto.Description,
        StockCount = dto.StockCount,
        CategoryId = dto.CategoryId
    };

    // PUT = full replace. RowVersion is not copied: the service sets it as the original value.
    public static void ApplyTo(this ProductUpdateDto dto, Product entity)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.Description = dto.Description;
        entity.StockCount = dto.StockCount;
        entity.CategoryId = dto.CategoryId;
    }
}
