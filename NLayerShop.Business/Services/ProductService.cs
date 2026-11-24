using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Business.Exceptions;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Querying;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<ProductCreateDto> _createValidator;
        private readonly IValidator<ProductUpdateDto> _updateValidator;
        private readonly IValidator<ProductQueryRequest> _queryValidator;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<ProductCreateDto> createValidator,
            IValidator<ProductUpdateDto> updateValidator,
            IValidator<ProductQueryRequest> queryValidator)
        {
            _uow = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
        }

        public async Task<PagedResponse<ProductDetailDto>> GetPagedAsync(ProductQueryRequest req, CancellationToken ct)
        {
            await _queryValidator.ValidateAndThrowAsync(req, ct);

            var filtered = _uow.Products.Query()
                .AsNoTracking()
                .ApplyFilters(req);

            var total = await filtered.CountAsync(ct);
            var items = await filtered.ApplySorting(req)
                               .Skip((req.Page - 1) * req.Size)
                               .Take(req.Size)
                               .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                               .ToListAsync(ct);

            return new PagedResponse<ProductDetailDto>
            {
                Items = items,
                Page = req.Page,
                Size = req.Size,
                TotalCount = total
            };
        }

        public async Task<ProductDetailDto> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _uow.Products.Query()
                       .AsNoTracking()
                       .Where(p => p.Id == id)
                       .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                       .FirstOrDefaultAsync(ct)
                   ?? throw new NotFoundException("Product", id);
        }

        public async Task<ProductDetailDto> CreateAsync(ProductCreateDto dto, CancellationToken ct)
        {
            await _createValidator.ValidateAndThrowAsync(dto, ct);
            await EnsureCategoryExistsAsync(dto.CategoryId, ct);

            var entity = _mapper.Map<Product>(dto);
            await _uow.Products.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return await GetByIdAsync(entity.Id, ct);
        }

        public async Task UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct)
        {
            await _updateValidator.ValidateAndThrowAsync(dto, ct);

            var entity = await _uow.Products.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Product", id);

            await EnsureCategoryExistsAsync(dto.CategoryId, ct);

            // Full replace: CategoryId null ise urun kategorisiz kalir.
            _mapper.Map(dto, entity);

            // Degisiklik olmasa bile UPDATE gonderilsin ki RowVersion kontrolu her zaman yapilsin.
            _uow.Products.Update(entity);
            _uow.Products.SetOriginalRowVersion(entity, Convert.FromBase64String(dto.RowVersion));

            // RowVersion eslesmezse DbUpdateConcurrencyException -> 409 Conflict
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _uow.Products.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Product", id);

            // Soft delete: global query filter sayesinde artik hicbir sorguda gorunmez.
            entity.IsActive = false;
            await _uow.SaveChangesAsync(ct);
        }

        private async Task EnsureCategoryExistsAsync(int? categoryId, CancellationToken ct)
        {
            if (categoryId is null)
                return;

            // Global query filter soft-delete edilmis kategorileri zaten disarida birakir.
            var exists = await _uow.Categories.Query().AnyAsync(c => c.Id == categoryId.Value, ct);
            if (!exists)
                throw new BusinessRuleException($"Category with id '{categoryId.Value}' does not exist.");
        }
    }
}
