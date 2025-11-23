using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLayerShop.Business.Interfaces
{
    public interface IProductService
    {
        public Task<PagedResponse<ProductDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct);
        public Task<ProductDetailDto?> GetByIdAsync(int id,CancellationToken ct);
        public Task<int> CreateAsync(ProductCreateDto dto,CancellationToken ct);
        public Task UpdateAsync(int id,ProductUpdateDto dto,CancellationToken ct);
        public Task DeleteAsync(int id, CancellationToken ct);
    }
}
