using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Business.Interfaces;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _uow = unitOfWork;
            _mapper = mapper;
        }

        public async Task<int> CreateAsync(ProductCreateDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<Product>(dto); // Dto -> Entity mapleme
            await _uow.Products.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _uow.Products.GetByIdAsync(id,ct)
                ?? throw new KeyNotFoundException("Product Not Found...");

            _uow.Products.Remove(entity);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var entity = await _uow.Products.Query()
                .Include(p=>p.Category)
                .FirstOrDefaultAsync(p => p.Id == id,ct);
            return entity is null ? null: _mapper.Map<ProductDetailDto?>(entity);
        }

        public async Task<PagedResponse<ProductDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct)
        {
            var q = _uow.Products.Query()
                     .Include(p => p.Category)
                     .OrderByDescending(p => p.Id);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((req.Page - 1) * req.Size)
                               .Take(req.Size)
                               .ToListAsync(ct);

            return new PagedResponse<ProductDetailDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDetailDto>>(items),
                Page = req.Page,
                Size = req.Size,
                TotalCount = total
            };
        }

        public async Task UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct)
        {
            var entity = await _uow.Products.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Product Not Found...");

            _mapper.Map(dto,entity);//Category disindakiler maplendi

            if (dto.CategoryId.HasValue && dto.RemoveCategory == true)
                throw new ValidationException("CategoryId verilirken RemoveCategory true olamaz.");

            if (dto.CategoryId.HasValue) {
                var newId=dto.CategoryId.Value;

                var exist = await _uow.Categories.GetByIdAsync(newId, ct);
                if (exist is null)
                    throw new ValidationException($"Category {newId} not found.");

                entity.CategoryId = newId;
                entity.Category = null;
            }else if (dto.RemoveCategory==true)
            {
                entity.CategoryId = null;       // ilişkiyi kaldır
                entity.Category = null;
            }
                await _uow.SaveChangesAsync(ct);
        }
    }
}
