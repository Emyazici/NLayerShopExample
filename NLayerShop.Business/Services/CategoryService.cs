using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Business.Exceptions;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Mapping;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CategoryCreateDto> _createValidator;
    private readonly IValidator<CategoryUpdateDto> _updateValidator;
    private readonly IValidator<PagedRequest> _pagedRequestValidator;

    public CategoryService(
        IUnitOfWork uow,
        IValidator<CategoryCreateDto> createValidator,
        IValidator<CategoryUpdateDto> updateValidator,
        IValidator<PagedRequest> pagedRequestValidator)
    {
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _pagedRequestValidator = pagedRequestValidator;
    }

    public async Task<PagedResponse<CategoryDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct)
    {
        await _pagedRequestValidator.ValidateAndThrowAsync(req, ct);

        var q = _uow.Categories.Query()
                               .AsNoTracking()
                               .OrderByDescending(c => c.Id);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((req.Page - 1) * req.Size)
                           .Take(req.Size)
                           .Select(CategoryMappings.ToDetailDto)
                           .ToListAsync(ct);

        return new PagedResponse<CategoryDetailDto>
        {
            Items = items,
            Page = req.Page,
            Size = req.Size,
            TotalCount = total
        };
    }

    public async Task<CategoryDetailDto> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _uow.Categories.Query()
                   .AsNoTracking()
                   .Where(c => c.Id == id)
                   .Select(CategoryMappings.ToDetailDto)
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException("Category", id);
    }

    public async Task<CategoryDetailDto> CreateAsync(CategoryCreateDto dto, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        var entity = dto.ToEntity();
        await _uow.Categories.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, ct);

        var entity = await _uow.Categories.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Category", id);

        dto.ApplyTo(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _uow.Categories.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Category", id);

        // Soft delete. Fiziksel silmedeki FK davranisini (SetNull) taklit ediyoruz:
        // bu kategoriye bagli urunler silinmez, kategorisiz kalir.
        var products = await _uow.Products.Query()
                                          .Where(p => p.CategoryId == id)
                                          .ToListAsync(ct);
        foreach (var product in products)
            product.CategoryId = null;

        entity.IsActive = false;
        await _uow.SaveChangesAsync(ct);
    }
}
