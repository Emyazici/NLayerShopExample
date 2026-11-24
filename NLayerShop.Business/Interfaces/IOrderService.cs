using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Orders;

namespace NLayerShop.Business.Interfaces
{
    public interface IOrderService
    {
        public Task<PagedResponse<OrderListItemDto>> GetPagedAsync(OrderQueryRequest req, CancellationToken ct);
        public Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct);
        public Task<OrderDetailDto> CreateAsync(OrderCreateDto dto, CancellationToken ct);
        public Task CancelAsync(int id, CancellationToken ct);
        public Task CompleteAsync(int id, CancellationToken ct);
    }
}
