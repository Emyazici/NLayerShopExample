using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Business.Exceptions;
using NLayerShop.Business.Interfaces;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;
using NLayerShop.Domain.Enums;

namespace NLayerShop.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<OrderCreateDto> _createValidator;
        private readonly IValidator<OrderQueryRequest> _queryValidator;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<OrderCreateDto> createValidator,
            IValidator<OrderQueryRequest> queryValidator)
        {
            _uow = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _queryValidator = queryValidator;
        }

        public async Task<PagedResponse<OrderListItemDto>> GetPagedAsync(OrderQueryRequest req, CancellationToken ct)
        {
            await _queryValidator.ValidateAndThrowAsync(req, ct);

            var q = _uow.Orders.Query().AsNoTracking();

            if (OrderQueryRequestValidator.TryParseStatus(req.Status, out var status))
                q = q.Where(o => o.Status == status);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(o => o.Id)
                               .Skip((req.Page - 1) * req.Size)
                               .Take(req.Size)
                               .ProjectTo<OrderListItemDto>(_mapper.ConfigurationProvider)
                               .ToListAsync(ct);

            return new PagedResponse<OrderListItemDto>
            {
                Items = items,
                Page = req.Page,
                Size = req.Size,
                TotalCount = total
            };
        }

        public async Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _uow.Orders.Query()
                       .AsNoTracking()
                       .Where(o => o.Id == id)
                       .ProjectTo<OrderDetailDto>(_mapper.ConfigurationProvider)
                       .FirstOrDefaultAsync(ct)
                   ?? throw new NotFoundException("Order", id);
        }

        public async Task<OrderDetailDto> CreateAsync(OrderCreateDto dto, CancellationToken ct)
        {
            // 1) Sekil kontrolu: en az 1 kalem, Quantity > 0, tekrar eden ProductId yok.
            await _createValidator.ValidateAndThrowAsync(dto, ct);

            // 2) Tum urunler TEK sorguda, tracking acik (stok burada dusulecek).
            //    Global query filter soft-delete edilmis urunleri zaten disarida birakir.
            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _uow.Products.Query()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            // 3) Once tum kurallar kontrol edilir, sonra degisiklik yapilir.
            foreach (var item in dto.Items)
            {
                if (!products.ContainsKey(item.ProductId))
                    throw new NotFoundException("Product", item.ProductId);
            }

            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                if (product.StockCount < item.Quantity)
                    throw new BusinessRuleException(
                        $"Insufficient stock for product '{product.Name}' (id {product.Id}): " +
                        $"requested {item.Quantity}, available {product.StockCount}.");
            }

            // 4) Stok dus, snapshot'li kalemleri olustur, toplami hesapla.
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                Status = OrderStatus.Pending
            };

            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                product.StockCount -= item.Quantity;

                var lineTotal = product.Price * item.Quantity;
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });
                order.TotalAmount += lineTotal;
            }

            await _uow.Orders.AddAsync(order, ct);

            // 5) TEK SaveChanges: Order INSERT + OrderItem INSERT'leri + Product stok UPDATE'leri
            //    ayni transaction'da. Biri basarisiz olursa hicbiri yazilmaz.
            // 6) Product.RowVersion concurrency token'dir: ayni urunu ayni anda okuyan iki siparisten
            //    ikincisi DbUpdateConcurrencyException alir -> 409 Conflict (stok asla eksiye dusmez).
            await _uow.SaveChangesAsync(ct);

            return await GetByIdAsync(order.Id, ct);
        }

        public async Task CancelAsync(int id, CancellationToken ct)
        {
            var order = await _uow.Orders.Query()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id, ct)
                ?? throw new NotFoundException("Order", id);

            if (order.Status != OrderStatus.Pending)
                throw new BusinessRuleException(
                    $"Only pending orders can be cancelled. Order '{order.OrderNumber}' is {order.Status}.");

            // Stoklar iade edilir. IgnoreQueryFilters: urun siparisten sonra soft-delete edilmis olsa bile
            // stogu geri yazilir; urun tekrar aktiflestirilirse sayim dogru olur.
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _uow.Products.Query()
                .IgnoreQueryFilters()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var item in order.Items)
            {
                // FK Restrict oldugu icin urun fiziksel olarak silinemez; yine de savunmaci kontrol.
                if (products.TryGetValue(item.ProductId, out var product))
                    product.StockCount += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;

            // Tek SaveChanges: siparis durumu + tum stok iadeleri atomik.
            await _uow.SaveChangesAsync(ct);
        }

        public async Task CompleteAsync(int id, CancellationToken ct)
        {
            var order = await _uow.Orders.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Order", id);

            if (order.Status != OrderStatus.Pending)
                throw new BusinessRuleException(
                    $"Only pending orders can be completed. Order '{order.OrderNumber}' is {order.Status}.");

            order.Status = OrderStatus.Completed;
            await _uow.SaveChangesAsync(ct);
        }

        // Ornek: ORD-20261124-9F3A1C2B (tarih UTC + GUID'in ilk 8 hex karakteri).
        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}
