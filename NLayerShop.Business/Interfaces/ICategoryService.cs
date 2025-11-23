using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Business.Interfaces
{
    public interface ICategoryService
    {
        public Task<PagedResponse<CategoryDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct);
        public Task<CategoryDetailDto?> GetByIdAsync(int id, CancellationToken ct);
        public Task<int> CreateAsync(CategoryCreateDto dto, CancellationToken ct);
        public Task UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken ct);
        public Task DeleteAsync(int id, CancellationToken ct);
    }
}
