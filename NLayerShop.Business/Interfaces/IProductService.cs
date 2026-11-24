using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.Business.Interfaces
{
    public interface IProductService
    {
        public Task<PagedResponse<ProductDetailDto>> GetPagedAsync(PagedRequest req, CancellationToken ct);
        public Task<ProductDetailDto> GetByIdAsync(int id, CancellationToken ct);
        public Task<ProductDetailDto> CreateAsync(ProductCreateDto dto, CancellationToken ct);
        public Task UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct);
        public Task DeleteAsync(int id, CancellationToken ct);
    }
}
