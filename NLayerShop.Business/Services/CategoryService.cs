using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Business.Interfaces;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CategoryDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct)
    {
        var q = _uow.Categories.Query().OrderByDescending(c => c.Id);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((req.Page - 1) * req.Size)
                           .Take(req.Size)
                           .ToListAsync(ct);

        return new PagedResponse<CategoryDetailDto>
        {
            Items = _mapper.Map<IEnumerable<CategoryDetailDto>>(items),
            Page = req.Page,
            Size = req.Size,
            TotalCount = total
        };
    }

    public async Task<CategoryDetailDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _uow.Categories.Query()
                                          .FirstOrDefaultAsync(c => c.Id == id, ct);
        return entity is null ? null : _mapper.Map<CategoryDetailDto>(entity);
    }

    public async Task<int> CreateAsync(CategoryCreateDto dto, CancellationToken ct)
    {
        var entity = _mapper.Map<Category>(dto);
        await _uow.Categories.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken ct)
    {
        var entity = await _uow.Categories.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException("Category not found");

        _mapper.Map(dto, entity);
        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _uow.Categories.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException("Category not found");

        _uow.Categories.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }
}
