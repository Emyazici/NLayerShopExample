using System.Linq.Expressions;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Mapping;

public static class CategoryMappings
{
    public static readonly Expression<Func<Category, CategoryDetailDto>> ToDetailDto = c => new CategoryDetailDto(
        c.Id,
        c.Name);

    public static Category ToEntity(this CategoryCreateDto dto) => new() { Name = dto.Name };

    public static void ApplyTo(this CategoryUpdateDto dto, Category entity)
    {
        entity.Name = dto.Name;
    }
}
